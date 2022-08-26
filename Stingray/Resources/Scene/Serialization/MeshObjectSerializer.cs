using VT2AssetLib.IO;
using VT2AssetLib.Serialization;
using VT2AssetLib.Stingray.Resources.Scene;
using VT2AssetLib.Stingray.Serialization;

namespace VT2AssetLib.Stingray.Resources.Scene.Serialization;

internal sealed class MeshObjectSerializer : SerializerBase<MeshObject>
{
    public static MeshObjectSerializer Default { get; } = new(null);

    public IDString32Serializer IDString32Serializer { get; }

    public MeshObjectSerializer(IDString32Serializer? idString32Serializer)
    {
        IDString32Serializer = idString32Serializer ?? IDString32Serializer.Default;
    }

    protected override void SerializeValue(PrimitiveWriter writer, MeshObject value)
    {
        writer.Serialize(IDString32Serializer, value.Name);
        writer.Write(value.NodeIndex);
        writer.Write(value.GeometryIndex);
        writer.Write(value.SkinIndex);
        writer.Write((uint)value.Flags);
        writer.Serialize(BoundingVolumeSerializer.Default, value.BoundingVolume);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out MeshObject result)
    {
        result = new MeshObject
        {
            Name = reader.Deserialize(IDString32Serializer),
            NodeIndex = reader.ReadInt32(),
            GeometryIndex = reader.ReadUInt32(),
            SkinIndex = reader.ReadUInt32(),
            Flags = (RenderableFlags)reader.ReadUInt32(),
            BoundingVolume = reader.Deserialize(BoundingVolumeSerializer.Default)
        };
    }
}