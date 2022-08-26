using VT2AssetLib.IO;
using VT2AssetLib.Serialization;
using VT2AssetLib.Stingray.Serialization;

namespace VT2AssetLib.Stingray.Resources.Serialization;

public class ResourceLocatorSerializer : SerializerBase<ResourceLocator>
{
    public static ResourceLocatorSerializer Default { get; } = new(null);

    public IDString64Serializer IDString64Serializer { get; }

    public ResourceLocatorSerializer(IDString64Serializer? idString64Serializer)
    {
        IDString64Serializer = idString64Serializer ?? IDString64Serializer.Default;
    }

    protected override void SerializeValue(PrimitiveWriter writer, ResourceLocator value)
    {
        IDString64Serializer.Serialize(writer, value.Type);
        IDString64Serializer.Serialize(writer, value.Name);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out ResourceLocator result)
    {
        var type = IDString64Serializer.Deserialize(reader);
        var name = IDString64Serializer.Deserialize(reader);

        result = new ResourceLocator(type, name);
    }
}