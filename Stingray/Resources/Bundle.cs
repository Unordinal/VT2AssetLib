using System.Diagnostics;
using VT2AssetLib.Collections;
using VT2AssetLib.IO;
using VT2AssetLib.IO.Extensions;
using VT2AssetLib.Stingray.IO;
using VT2AssetLib.Stingray.Resources.Serialization;
using VT2AssetLib.Stingray.Serialization;

namespace VT2AssetLib.Stingray.Resources;

public sealed class Bundle : IDisposable
{
    private const int MaxSegmentLength = 0x10000;
    private const int MaxPropertyCount = 32;
    private const int PropertySectionSize = MaxPropertyCount * sizeof(ulong);

    //internal static Regex BundleNameRegex { get; } = new(@"^(?<name>[0-9a-z]{16})(\.patch_(?<patch>\d{3}))?(?<stream>\.stream)?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

    /// <summary>
    /// Gets the bundle's full, absolute path. May be <see langword="null"/> if the bundle was created via stream.
    /// </summary>
    public string? Name { get; private set; }

    public BundleVersion Version { get; private set; }

    public IReadOnlyList<IDString64> Properties => _properties.AsReadOnly();

    public IEnumerable<ResourceLocator> Resources => _resourceMap.SelectMany(p => p.Value);

    private readonly Stream _stream;
    private readonly IDString64Serializer _idString64Serializer;
    private readonly SegmentDecompressor _decompressor;
    private bool _disposed;

    private readonly List<IDString64> _properties;
    private BundleEntryMeta[] _entryMetas;

    private readonly OrderedDictionary<Bundle, IEnumerable<ResourceLocator>> _resourceMap;
    private SemaphoreSlim _semaphore = new(1);

    private Bundle(string bundlePath, IDStringRepository? idStringRepository = null)
    {
        Name = bundlePath;
        _stream = File.OpenRead(bundlePath);
        _idString64Serializer = new IDString64Serializer(idStringRepository);
        _decompressor = new SegmentDecompressor(_stream, MaxSegmentLength);
        _properties = new List<IDString64>(MaxPropertyCount);

        _entryMetas = null!; // Set by factory methods calling ReadHeaderAndMetas.
        _resourceMap = new OrderedDictionary<Bundle, IEnumerable<ResourceLocator>>();
    }

    /// <summary>
    /// Asynchronously reads the specified resources from this bundle and any applied patch bundles.
    /// </summary>
    /// <param name="resources"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<IList<BundledResource>> ReadResourcesAsync(IEnumerable<ResourceLocator> resources, CancellationToken cancellationToken = default)
    {
        var resourcesToRead = resources.ToHashSet();
        var bundlesToRead = _resourceMap
            .Where(p => p.Value.Intersect(resources).Any())
            .Select(p => p.Key);

        var resourceLocatorSerializer = new ResourceLocatorSerializer(_idString64Serializer);

        var readResources = new List<BundledResource>(resourcesToRead.Count);
        foreach (var bundle in bundlesToRead)
        {
            await bundle._semaphore.WaitAsync(cancellationToken);

            try
            {
                using var decompStream = bundle.CreateEntryDecompressorStream();
                using var decompReader = new PrimitiveReader(decompStream, ByteOrder.LittleEndian);

                for (int i = 0; i < bundle._entryMetas.Length; i++)
                {
                    var entryTypeAndHash = await decompReader.DeserializeAsync(resourceLocatorSerializer, cancellationToken).ConfigureAwait(false);
                    if (!resourcesToRead.Contains(entryTypeAndHash))
                    {
                        await SkipEntryAsync(decompReader, entryTypeAndHash, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    var entry = await ReadEntryAsync(decompReader, entryTypeAndHash, cancellationToken).ConfigureAwait(false);
                    readResources.Add(new BundledResource(entry));
                }
            }
            finally
            {
                bundle._semaphore.Release();
            }
        }

        return readResources;
    }

    /// <summary>
    /// Loads the patch bundle at the given path and adds it to this bundle's resource map.
    /// </summary>
    /// <param name="patchBundlePath"></param>
    /// <exception cref="ArgumentException"></exception>
    public void ApplyPatch(string patchBundlePath)
    {
        if (string.IsNullOrEmpty(patchBundlePath))
            throw new ArgumentException("The path is null or white space.", nameof(patchBundlePath));

        var patchBundle = Open(patchBundlePath);
        ApplyPatch(patchBundle);
    }

    /// <summary>
    /// Asynchronously loads the patch bundle at the given path and adds it to this bundle's resource map.
    /// </summary>
    /// <param name="patchBundlePath"></param>
    /// <exception cref="ArgumentException"></exception>
    public async ValueTask ApplyPatchAsync(string patchBundlePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(patchBundlePath))
            throw new ArgumentException("The path is null or white space.", nameof(patchBundlePath));

        var patchBundle = await OpenAsync(patchBundlePath, _idString64Serializer.IDStringRepository, false, cancellationToken).ConfigureAwait(false);
        ApplyPatch(patchBundle);
    }

    /// <summary>
    /// Adds the given patch bundle's resources to this bundle's resource map.
    /// <para/>
    /// Note: this instance takes ownership of the passed-in patch bundle.
    /// </summary>
    /// <param name="patchBundlePath"></param>
    /// <exception cref="ArgumentException"></exception>
    public void ApplyPatch(Bundle patchBundle)
    {
        ArgumentNullException.ThrowIfNull(patchBundle);
        if (patchBundle._resourceMap.ContainsKey(this))
            throw new ArgumentException("The specified patch bundle contains a resource from this bundle as a dependency.", nameof(patchBundle));

        var resourcesToAdd = patchBundle.GetExistingResources().Except(this.GetExistingResources());
        _resourceMap.Add(patchBundle, resourcesToAdd);
    }

    public void ApplyPatches(IEnumerable<string> patchBundlePaths)
    {
        ArgumentNullException.ThrowIfNull(patchBundlePaths);
        foreach (var patchBundlePath in patchBundlePaths)
            ApplyPatch(patchBundlePath);
    }

    public async ValueTask ApplyPatchesAsync(IEnumerable<string> patchBundlePaths, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patchBundlePaths);
        foreach (var patchBundlePath in patchBundlePaths)
            await ApplyPatchAsync(patchBundlePath, cancellationToken);
    }

    /// <summary>
    /// Adds the given patch bundles' resources to this bundle's resource map.
    /// <para/>
    /// Note: this instance takes ownership of all of the passed-in patch bundles.
    /// </summary>
    /// <param name="patchBundlePath"></param>
    /// <exception cref="ArgumentException"></exception>
    public void ApplyPatches(IEnumerable<Bundle> patchBundles)
    {
        ArgumentNullException.ThrowIfNull(patchBundles);
        foreach (var patchBundle in patchBundles)
            ApplyPatch(patchBundle);
    }

    private void ReadHeaderAndMetas()
    {
        ReadHeaderAndMetasAsync().GetAwaiter().GetResult(); // TODO: Actually implement.
    }

    private async Task ReadHeaderAndMetasAsync(CancellationToken cancellationToken = default)
    {
        Debug.Assert(_stream.Position == 0);
        Debug.Assert(_entryMetas is null);
        using var reader = new PrimitiveReader(_stream, ByteOrder.LittleEndian, true);

        Version = (BundleVersion)reader.ReadUInt32();
        if (Version < BundleVersion.VT2X)
            throw new InvalidDataException($"Unsupported bundle file version '{Version}'");

        long bundleSize = reader.ReadInt64();

        using var decompStream = new SegmentDecompressorStream(_decompressor, 2, true);
        using var decompReader = new PrimitiveReader(decompStream, ByteOrder.LittleEndian);

        int entryCount = decompReader.ReadInt32();
        if (entryCount < 0)
            throw new InvalidDataException($"Invalid entry count read: {entryCount}");

        for (int i = 0; i < MaxPropertyCount; i++)
        {
            IDString64 property = await decompReader.DeserializeAsync(_idString64Serializer, cancellationToken).ConfigureAwait(false);
            if (!property.IsEmpty)
                _properties.Add(property);
        }

        var entryMetaSerializer = new BundleEntryMetaSerializer(Version, _idString64Serializer);
        _entryMetas = ArrayUtil.Create<BundleEntryMeta>(entryCount);
        for (int i = 0; i < _entryMetas.Length; i++)
        {
            var entryMeta = await decompReader.DeserializeAsync(entryMetaSerializer, cancellationToken).ConfigureAwait(false);
            _entryMetas[i] = entryMeta;
        }

        _resourceMap.Add(this, _entryMetas.Select(em => em.Locator));
    }

    private int GetEntryMetaSize()
    {
        return Version switch
        {
            BundleVersion.VT1 => 16,
            BundleVersion.VT2 => 20,
            BundleVersion.VT2X => 24,
            _ => throw new InvalidOperationException("The bundle version is invalid.")
        };
    }

    private int GetEntryMetaSectionSize()
    {
        return GetEntryMetaSize() * _entryMetas.Length;
    }

    /// <summary>
    /// Gets the resources only in this bundle that have not been marked as deleted or moved.
    /// Excludes resources added via applied patches.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<ResourceLocator> GetExistingResources()
    {
        return _entryMetas
            .Where(em => em.Flag is not ResourceFlag.Deleted and not ResourceFlag.Moved)
            .Select(em => em.Locator);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _semaphore.Dispose();
            _stream.Dispose();
            _decompressor.Dispose();
            foreach (var patchBundle in _resourceMap.Keys.Where(b => b != this))
                patchBundle.Dispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Creates a decompressor stream and seeks to the start of the BundleEntry section before returning the created stream.
    /// </summary>
    /// <returns></returns>
    private SegmentDecompressorStream CreateEntryDecompressorStream()
    {
        _stream.Seek(12, SeekOrigin.Begin); // Skip: Version + BundleSize
        var decompStream = new SegmentDecompressorStream(_decompressor, 2, true);
        decompStream.SkipBytes(4 + PropertySectionSize + GetEntryMetaSectionSize()); // Skip: EntryCount + Properties + EntryMetas

        return decompStream;
    }

    private static BundleEntry ReadEntry(PrimitiveReader reader, ResourceLocator resourceTypeAndName)
    {
        var entry = new BundleEntry
        {
            Locator = resourceTypeAndName,
            VariantCount = reader.ReadUInt32(),
            StreamOffset = reader.ReadUInt32(),
        };

        entry.Variants = ArrayUtil.CreateAndPopulate<BundleEntryVariant>(entry.VariantCount);
        foreach (var variant in entry.Variants)
        {
            variant.Language = (ResourceLanguage)reader.ReadUInt32();
            variant.Size = reader.ReadUInt32();
            variant.StreamSize = reader.ReadUInt32();
        }

        foreach (var variant in entry.Variants)
        {
            variant.Data = reader.ReadBytes((int)variant.Size);
        }

        return entry;
    }

    private static async ValueTask<BundleEntry> ReadEntryAsync(PrimitiveReader reader, ResourceLocator resourceTypeAndName, CancellationToken cancellationToken = default)
    {
        var entry = new BundleEntry
        {
            Locator = resourceTypeAndName,
            VariantCount = reader.ReadUInt32(),
            StreamOffset = reader.ReadUInt32(),
        };

        entry.Variants = ArrayUtil.CreateAndPopulate<BundleEntryVariant>(entry.VariantCount);
        foreach (var variant in entry.Variants)
        {
            variant.Language = (ResourceLanguage)reader.ReadUInt32();
            variant.Size = reader.ReadUInt32();
            variant.StreamSize = reader.ReadUInt32();
        }

        foreach (var variant in entry.Variants)
        {
            variant.Data = await reader.ReadBytesAsync((int)variant.Size, cancellationToken).ConfigureAwait(false);
        }

        return entry;
    }

    private static void SkipEntry(PrimitiveReader reader, ResourceLocator resourceTypeAndName)
    {
        // TODO: Use resourceTypeAndName to check meta for variant size and use that to SkipBytes instead.
        uint variantCount = reader.ReadUInt32();
        uint streamOffset = reader.ReadUInt32();

        int bytesToSkip = 0;
        for (int i = 0; i < variantCount; i++)
        {
            ResourceLanguage variantLanguage = (ResourceLanguage)reader.ReadUInt32();
            uint variantSize = reader.ReadUInt32();
            uint variantStreamSize = reader.ReadUInt32();

            bytesToSkip += (int)variantSize;
        }

        reader.BaseStream.SkipBytes(bytesToSkip);
    }

    private static async ValueTask SkipEntryAsync(PrimitiveReader reader, ResourceLocator resourceTypeAndName, CancellationToken cancellationToken = default)
    {
        // TODO: Use resourceTypeAndName to check meta for variant size and use that to SkipBytes instead.
        uint variantCount = reader.ReadUInt32();
        uint streamOffset = reader.ReadUInt32();

        int bytesToSkip = 0;
        for (int i = 0; i < variantCount; i++)
        {
            ResourceLanguage variantLanguage = (ResourceLanguage)reader.ReadUInt32();
            uint variantSize = reader.ReadUInt32();
            uint variantStreamSize = reader.ReadUInt32();

            bytesToSkip += (int)variantSize;
        }

        await reader.BaseStream.SkipBytesAsync(bytesToSkip, true, cancellationToken);
    }

    public static Bundle Open(string bundlePath, IDStringRepository? idStringRepository = null, bool findAndApplyPatches = true)
    {
        if (string.IsNullOrWhiteSpace(bundlePath))
            throw new ArgumentException("The given path is null or white space.", nameof(bundlePath));
        if (!File.Exists(bundlePath))
            throw new ArgumentException("The specified path does not exist.", nameof(bundlePath));

        var bundle = new Bundle(bundlePath, idStringRepository);
        bundle.ReadHeaderAndMetas();

        if (findAndApplyPatches && !Path.GetFileName(bundlePath).Contains(".patch_"))
        {
            var patchBundlePaths = FindPatchFiles(bundlePath);
            bundle.ApplyPatches(patchBundlePaths);
        }

        return bundle;
    }

    public static async Task<Bundle> OpenAsync(
        string bundlePath,
        IDStringRepository? idStringRepository = null,
        bool findAndApplyPatches = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bundlePath))
            throw new ArgumentException("The given path is null or white space.", nameof(bundlePath));
        if (!File.Exists(bundlePath))
            throw new ArgumentException("The specified path does not exist.", nameof(bundlePath));

        var bundle = new Bundle(bundlePath, idStringRepository);
        await bundle.ReadHeaderAndMetasAsync(cancellationToken).ConfigureAwait(false);

        if (findAndApplyPatches && !Path.GetFileName(bundlePath).Contains(".patch_"))
        {
            var patchBundlePaths = FindPatchFiles(bundlePath);
            await bundle.ApplyPatchesAsync(patchBundlePaths, cancellationToken).ConfigureAwait(false);
        }

        return bundle;
    }

    private static IEnumerable<string> FindPatchFiles(string bundlePath)
    {
        string bundleName = Path.GetFileName(bundlePath);
        if (string.IsNullOrEmpty(bundleName))
            throw new ArgumentException("There is no bundle name in the given path.", nameof(bundlePath));

        string? bundleDirectory = Path.GetDirectoryName(bundlePath);
        if (string.IsNullOrEmpty(bundleDirectory))
            throw new ArgumentException("The bundle path is not supported.", nameof(bundlePath));

        return Directory.EnumerateFiles(bundleDirectory, $"{bundleName}.patch_*") // May return '.patch_###.stream' files so we need to do an additional 'Where' filter.
            .Where(f => Path.GetExtension(f).StartsWith(".patch", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f); // Guarantee alpabetical order (.patch_001, .patch_002, ...)
    }
}