using VT2AssetLib.IO;
using VT2AssetLib.Serialization;
using VT2AssetLib.Stingray.Resources.Scene;

namespace VT2AssetLib.Stingray.Resources.Scene.Serialization;

internal sealed class BatchRangeSerializer : SerializerBase<BatchRange>
{
    public static BatchRangeSerializer Default { get; } = new();

    protected override void SerializeValue(PrimitiveWriter writer, BatchRange value)
    {
        writer.Write(value.MaterialIndex);
        writer.Write(value.Start);
        writer.Write(value.Size);
        writer.Write(value.BoneSet);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out BatchRange result)
    {
        result = new BatchRange
        {
            MaterialIndex = reader.ReadUInt32(),
            Start = reader.ReadUInt32(),
            Size = reader.ReadUInt32(),
            BoneSet = reader.ReadUInt32(),
        };
    }
}