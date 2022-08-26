using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT2AssetLib.IO;
using VT2AssetLib.Serialization;
using VT2AssetLib.Stingray.Serialization;

namespace VT2AssetLib.Stingray.Resources.Serialization;
internal sealed class BundleEntrySerializer : SerializerBase<BundleEntry>
{
    public static BundleEntrySerializer Default { get; } = new(null);

    public IDString64Serializer IDString64Serializer { get; }

    private ResourceLocatorSerializer ResourceLocatorSerializer { get; }

    public BundleEntrySerializer(IDString64Serializer? idString64Serializer)
    {
        IDString64Serializer = idString64Serializer ?? IDString64Serializer.Default;
        ResourceLocatorSerializer = new ResourceLocatorSerializer(IDString64Serializer);
    }

    protected override void SerializeValue(PrimitiveWriter writer, BundleEntry value)
    {
        throw new NotImplementedException();
    }

    protected override void DeserializeValue(PrimitiveReader reader, out BundleEntry result)
    {
        result = new BundleEntry
        {
            Locator = reader.Deserialize(ResourceLocatorSerializer),
            VariantCount = reader.ReadUInt32(),
            StreamOffset = reader.ReadUInt32(),
        };


        result.Variants = ArrayUtil.CreateAndPopulate<BundleEntryVariant>(result.VariantCount);
        foreach (var variant in result.Variants)
        {
            variant.Language = (ResourceLanguage)reader.ReadUInt32();
            variant.Size = reader.ReadUInt32();
            variant.StreamSize = reader.ReadUInt32();
        }

        foreach (var variant in result.Variants)
        {
            variant.Data = reader.ReadBytes((int)variant.Size);
        }
    }
}
