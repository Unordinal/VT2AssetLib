using VT2AssetLib.IO;
using VT2AssetLib.Serialization;

namespace VT2AssetLib.Stingray.Serialization;

// TODO: Should this exist? Should we instead just handle arrays manually when we need them? This is kind of inefficient for reading
// primitives too, which means there's a discrepancy between how to handle arrays of primitives vs arrays of objects - which isn't great.
/// <summary>
/// Serializes an array of the specified type by using the given <see cref="SerializerBase{T}"/>.
/// The array is prefixed with the length.
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class ArraySerializer<T> : SerializerBase<T[]>
{
    public SerializerBase<T> ValueSerializer { get; }

    public ArraySerializer(SerializerBase<T> valueSerializer)
    {
        ValueSerializer = valueSerializer;
    }

    protected override void SerializeValue(PrimitiveWriter writer, T[] value)
    {
        writer.Write((uint)value.Length);
        for (int i = 0; i < value.Length; i++)
            writer.Serialize(ValueSerializer, value[i]);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out T[] result)
    {
        uint count = reader.ReadUInt32();
        result = ArrayUtil.Create<T>(count);
        for (int i = 0; i < count; i++)
            result[i] = reader.Deserialize(ValueSerializer);
    }
}