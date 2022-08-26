using VT2AssetLib.IO;

namespace VT2AssetLib.Serialization;

internal interface ISerializer<T>
{
    void Serialize(Stream stream, T value, ByteOrder byteOrder);

    void Serialize(PrimitiveWriter writer, T value);

    T Deserialize(Stream stream, ByteOrder byteOrder);

    T Deserialize(PrimitiveReader reader);
}