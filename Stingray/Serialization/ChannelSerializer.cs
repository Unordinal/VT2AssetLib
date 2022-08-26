using VT2AssetLib.IO;
using VT2AssetLib.Serialization;

namespace VT2AssetLib.Stingray.Serialization;

internal class ChannelSerializer : SerializerBase<Channel>
{
    public static ChannelSerializer Default { get; } = new();

    protected override void SerializeValue(PrimitiveWriter writer, Channel value)
    {
        writer.Write((uint)value.Component);
        writer.Write((uint)value.Type);
        writer.Write(value.Set);
        writer.Write(value.Stream);
        writer.Write(value.IsInstance);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out Channel result)
    {
        result = new Channel
        {
            Component = (VertexComponent)reader.ReadUInt32(),
            Type = (ChannelType)reader.ReadUInt32(),
            Set = reader.ReadUInt32(),
            Stream = reader.ReadUInt32(),
            IsInstance = reader.ReadByte(),
        };
    }
}