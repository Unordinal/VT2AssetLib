using VT2AssetLib.IO;
using VT2AssetLib.Serialization;

namespace VT2AssetLib.Stingray.Serialization;

internal class IndexBufferSerializer : SerializerBase<IndexBuffer>
{
    public static IndexBufferSerializer Default { get; } = new();

    protected override void SerializeValue(PrimitiveWriter writer, IndexBuffer value)
    {
        writer.Write((uint)value.Validity);
        writer.Write((uint)value.StreamType);
        writer.Write((uint)value.IndexFormat);
        writer.Write(value.IndexCount);
        writer.Write((uint)value.Data.Length);
        writer.Write(value.Data);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out IndexBuffer result)
    {
        Validity validity = (Validity)reader.ReadUInt32();
        StreamType streamType = (StreamType)reader.ReadUInt32();
        IndexFormat indexFormat = (IndexFormat)reader.ReadUInt32();
        uint count = reader.ReadUInt32();

        int indexBytesLength = reader.ReadInt32();
        byte[] indices = reader.ReadBytes(indexBytesLength);

        result = new IndexBuffer
        {
            Validity = validity,
            StreamType = streamType,
            IndexFormat = indexFormat,
            IndexCount = count,
            Data = indices,
        };
    }
}