using System.Diagnostics;
using VT2AssetLib.IO;
using VT2AssetLib.Serialization;
using VT2AssetLib.Stingray.Serialization;

namespace VT2AssetLib.Stingray.Resources.Serialization;

internal sealed class BundleEntryMetaSerializer : SerializerBase<BundleEntryMeta>
{
    public IDString64Serializer IDString64Serializer { get; }

    private ResourceLocatorSerializer ResourceLocatorSerializer { get; }

    private readonly BundleVersion _bundleVersion;

    public BundleEntryMetaSerializer(BundleVersion bundleVersion, IDString64Serializer? idString64Serializer)
    {
        _bundleVersion = bundleVersion;
        IDString64Serializer = idString64Serializer ?? IDString64Serializer.Default;
        ResourceLocatorSerializer = new ResourceLocatorSerializer(IDString64Serializer);
    }

    protected override void SerializeValue(PrimitiveWriter writer, BundleEntryMeta value)
    {
        writer.Serialize(ResourceLocatorSerializer, value.Locator);

        if (value.Flag.HasValue)
        {
            Debug.Assert(_bundleVersion >= BundleVersion.VT2);
            writer.Write((uint)value.Flag);
        }

        if (value.Size.HasValue)
        {
            Debug.Assert(_bundleVersion >= BundleVersion.VT2X);
            writer.Write(value.Size.Value);
        }
    }

    protected override void DeserializeValue(PrimitiveReader reader, out BundleEntryMeta result)
    {
        result = new BundleEntryMeta
        {
            Locator = reader.Deserialize(ResourceLocatorSerializer),
        };

        if (_bundleVersion >= BundleVersion.VT2)
            result.Flag = (ResourceFlag)reader.ReadUInt32();

        if (_bundleVersion >= BundleVersion.VT2X)
            result.Size = reader.ReadUInt32();
    }
}