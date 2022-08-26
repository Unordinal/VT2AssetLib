using VT2AssetLib.IO;

namespace VT2AssetLib.Serialization;

internal interface IAsyncSerializer<T>
{
    ValueTask SerializeAsync(Stream stream, T value, ByteOrder byteOrder, CancellationToken cancellationToken);

    ValueTask SerializeAsync(PrimitiveWriter writer, T value, CancellationToken cancellationToken);

    ValueTask<T> DeserializeAsync(Stream stream, ByteOrder byteOrder, CancellationToken cancellationToken);

    ValueTask<T> DeserializeAsync(PrimitiveReader reader, CancellationToken cancellationToken);
}