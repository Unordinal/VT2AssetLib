namespace VT2AssetLib.Stingray.Resources;

public sealed class BundledResource
{
    /// <summary>
    /// Gets a <see cref="ResourceLocator"/> which holds the resource's name and type.
    /// </summary>
    public ResourceLocator Locator => _bundleEntry.Locator;

    /// <summary>
    /// Gets the resource's name. Equivalent to accessing <see cref="ResourceLocator.Name"/> through <see cref="Locator"/>.
    /// </summary>
    public IDString64 Name => Locator.Name;

    /// <summary>
    /// Gets the resource's type. Equivalent to accessing <see cref="ResourceLocator.Type"/> through <see cref="Locator"/>.
    /// </summary>
    public IDString64 Type => Locator.Type;

    /// <summary>
    /// Gets the file variants for this resource. (Such as the same file in another language.)
    /// </summary>
    public BundledResourceVariant[] Variants { get; }

    private readonly BundleEntry _bundleEntry;

    internal BundledResource(BundleEntry bundleEntry)
    {
        _bundleEntry = bundleEntry;
        Variants = bundleEntry.Variants.Select(e => new BundledResourceVariant(e)).ToArray();
    }

    /* /// <summary>
     /// Saves this resource and its variants to the specified path.
     /// </summary>
     /// <param name="path">The path to save to. If this is a directory, the resource will be saved with its original name.</param>
     /// <param name="keepFolderStructure">
     ///     If <see langword="true"/>, the resource will have its original, relative path if it has one and ".unknown\" if it doesn't;
     ///     otherwise, the resource will be saved directly to the specified path.
     /// </param>
     /// <returns></returns>
     public async Task SaveToFileAsync(string path, bool keepFolderStructure = true)
     {
         await SaveToFileAsync(path, keepFolderStructure).ConfigureAwait(false);
     }*/

    /// <summary>
    /// Saves this resource and its variants to the specified path, decompiling each variant by using the passed resource decompiler.
    /// </summary>
    /// <param name="decompiler">The decompiler that will be used to transform the data of each resource into a new form.</param>
    /// <inheritdoc cref="SaveToFileAsync(string, bool)"/>
    public async Task SaveToFileAsync(string path, bool keepFolderStructure = true)
    {
        ArgumentNullException.ThrowIfNull(path);
        throw new NotImplementedException();
    }
}