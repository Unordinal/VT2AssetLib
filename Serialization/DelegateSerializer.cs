using VT2AssetLib.IO;

namespace VT2AssetLib.Serialization;

internal class DelegateSerializer<T> : SerializerBase<T>
{
    private readonly Action<PrimitiveWriter, T>? _serializer;

    private readonly Func<PrimitiveReader, T>? _deserializer;

    public DelegateSerializer(Action<PrimitiveWriter, T>? serializer = null, Func<PrimitiveReader, T>? deserializer = null)
    {
        _serializer = serializer;
        _deserializer = deserializer;
    }

    protected override void SerializeValue(PrimitiveWriter writer, T value)
    {
        if (_serializer is null)
            throw new NotImplementedException();

        _serializer.Invoke(writer, value);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out T result)
    {
        if (_deserializer is null)
            throw new NotImplementedException();

        result = _deserializer.Invoke(reader);
    }
}