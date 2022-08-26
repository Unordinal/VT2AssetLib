using VT2AssetLib.IO;

namespace VT2AssetLib.Serialization;

public abstract class SerializerBase<T> : ISerializer<T>, IAsyncSerializer<T>
{
    public void Serialize(Stream stream, T value, ByteOrder byteOrder)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var writer = new PrimitiveWriter(stream, byteOrder, true);
        Serialize(writer, value);
    }

    public void Serialize(PrimitiveWriter writer, T value)
    {
        ArgumentNullException.ThrowIfNull(writer);

        SerializeValue(writer, value);
    }

    public T Deserialize(Stream stream, ByteOrder byteOrder)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var reader = new PrimitiveReader(stream, byteOrder, true);
        return Deserialize(reader);
    }

    public T Deserialize(PrimitiveReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        DeserializeValue(reader, out T result);
        return result;
    }

    public ValueTask SerializeAsync(Stream stream, T value, ByteOrder byteOrder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var writer = new PrimitiveWriter(stream, byteOrder, true);
        return SerializeAsync(writer, value, cancellationToken);
    }

    public ValueTask SerializeAsync(PrimitiveWriter writer, T value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(writer);

        return SerializeValueAsync(writer, value, cancellationToken);
    }

    public ValueTask<T> DeserializeAsync(Stream stream, ByteOrder byteOrder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var reader = new PrimitiveReader(stream, byteOrder, true);
        return DeserializeAsync(reader, cancellationToken);
    }

    public async ValueTask<T> DeserializeAsync(PrimitiveReader reader, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);

        await DeserializeValueAsync(reader, out T result, cancellationToken).ConfigureAwait(false);
        return result;
    }

    protected abstract void SerializeValue(PrimitiveWriter writer, T value);

    protected abstract void DeserializeValue(PrimitiveReader reader, out T result);

    protected virtual ValueTask SerializeValueAsync(PrimitiveWriter writer, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            SerializeValue(writer, value);
            return ValueTask.CompletedTask;
        }
        catch (Exception exc)
        {
            return ValueTask.FromException(exc);
        }
    }

    protected virtual ValueTask DeserializeValueAsync(PrimitiveReader reader, out T result, CancellationToken cancellationToken = default)
    {
        try
        {
            DeserializeValue(reader, out result);
            return ValueTask.CompletedTask;
        }
        catch (Exception exc)
        {
            result = default!;
            return ValueTask.FromException(exc);
        }
    }
}