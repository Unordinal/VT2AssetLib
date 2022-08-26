using System.Diagnostics;
using VT2AssetLib.IO;
using VT2AssetLib.Serialization;

namespace VT2AssetLib.Stingray.Serialization;

internal class VertexBuffersSerializer : SerializerBase<VertexBuffer[]>
{
    public static VertexBuffersSerializer Default { get; } = new();

    private ChannelSerializer ChannelSerializer { get; } = ChannelSerializer.Default;

    protected override void SerializeValue(PrimitiveWriter writer, VertexBuffer[] value)
    {
        writer.Write(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            var buffer = value[i];
            writer.Write(buffer.Data.Length);
            writer.Write(buffer.Data);
            writer.Write((uint)buffer.Validity);
            writer.Write((uint)buffer.StreamType);
            writer.Write(buffer.Count);
            writer.Write(buffer.Stride);
        }

        for (int i = 0; i < value.Length; i++)
        {
            var buffer = value[i];
            writer.Serialize(ChannelSerializer, buffer.Channel);
        }
    }

    protected override void DeserializeValue(PrimitiveReader reader, out VertexBuffer[] result)
    {
        int bufferCount = reader.ReadInt32();
        result = new VertexBuffer[bufferCount];

        for (int i = 0; i < bufferCount; i++)
        {
            int dataLength = reader.ReadInt32();
            Debug.Assert(dataLength >= 0, "VertexBuffer dataLength < 0");

            byte[] data = reader.ReadBytes(dataLength);
            Validity validity = (Validity)reader.ReadUInt32();
            StreamType streamType = (StreamType)reader.ReadUInt32();
            uint count = reader.ReadUInt32();
            uint stride = reader.ReadUInt32();

            result[i] = new VertexBuffer
            {
                Data = data,
                Validity = validity,
                StreamType = streamType,
                Count = count,
                Stride = stride,
            };
        }

        int channelCount = reader.ReadInt32();
        Debug.Assert(channelCount == bufferCount, "VertexBuffer channelCount != bufferCount");

        for (int i = 0; i < channelCount; i++)
        {
            result[i].Channel = reader.Deserialize(ChannelSerializer);
        }
    }
}