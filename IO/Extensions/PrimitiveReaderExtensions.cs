using System.Numerics;
using System.Text;
using VT2AssetLib.Serialization;

namespace VT2AssetLib.IO;

public static class PrimitiveReaderExtensions
{
    private const int MaxStackAlloc = 128;

    [ThreadStatic]
    private static readonly StringBuilder _sb = new();

    /// <summary>
    /// Reads a null-terminated ASCII string from the underlying stream.
    /// </summary>
    /// <returns></returns>
    public static string ReadCString(this PrimitiveReader reader)
    {
        _sb.Clear();

        byte readByte;
        while ((readByte = reader.ReadByte()) != 0)
            _sb.Append((char)readByte);

        return _sb.ToString();
    }

    /// <summary>
    /// Reads a length-prefixed ASCII string from the underlying stream.
    /// </summary>
    /// <returns></returns>
    public static string ReadPString16(this PrimitiveReader reader)
    {
        return reader.ReadPString16(reader.ByteOrder);
    }

    /// <summary>
    /// Reads a length-prefixed ASCII string from the underlying stream.
    /// </summary>
    /// <returns></returns>
    public static string ReadPString16(this PrimitiveReader reader, ByteOrder byteOrder)
    {
        int length = reader.ReadInt16(byteOrder);
        return reader.ReadString(length);
    }

    /// <summary>
    /// Reads a length-prefixed ASCII string from the underlying stream.
    /// </summary>
    /// <returns></returns>
    public static string ReadPString32(this PrimitiveReader reader)
    {
        return reader.ReadPString32(reader.ByteOrder);
    }

    /// <summary>
    /// Reads a length-prefixed ASCII string from the underlying stream.
    /// </summary>
    /// <returns></returns>
    public static string ReadPString32(this PrimitiveReader reader, ByteOrder byteOrder)
    {
        int length = reader.ReadInt32(byteOrder);
        return reader.ReadString(length);
    }

    /// <summary>
    /// Reads an ASCII string with the specified length from the underlying stream.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static string ReadString(this PrimitiveReader reader, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");

        using StackAllocHelper<byte> buffer = length <= MaxStackAlloc
            ? new(stackalloc byte[length])
            : new(length);

        reader.ReadExactly(buffer.Span);
        return Encoding.ASCII.GetString(buffer.Span);
    }

    public static Vector2 ReadVector2(this PrimitiveReader reader)
    {
        return reader.ReadVector2(reader.ByteOrder);
    }

    public static Vector2 ReadVector2(this PrimitiveReader reader, ByteOrder byteOrder)
    {
        return new Vector2
        {
            X = reader.ReadSingle(byteOrder),
            Y = reader.ReadSingle(byteOrder),
        };
    }

    public static Vector3 ReadVector3(this PrimitiveReader reader)
    {
        return reader.ReadVector3(reader.ByteOrder);
    }

    public static Vector3 ReadVector3(this PrimitiveReader reader, ByteOrder byteOrder)
    {
        return new Vector3
        {
            X = reader.ReadSingle(byteOrder),
            Y = reader.ReadSingle(byteOrder),
            Z = reader.ReadSingle(byteOrder),
        };
    }

    public static Vector4 ReadVector4(this PrimitiveReader reader)
    {
        return reader.ReadVector4(reader.ByteOrder);
    }

    public static Vector4 ReadVector4(this PrimitiveReader reader, ByteOrder byteOrder)
    {
        return new Vector4
        {
            X = reader.ReadSingle(byteOrder),
            Y = reader.ReadSingle(byteOrder),
            Z = reader.ReadSingle(byteOrder),
            W = reader.ReadSingle(byteOrder),
        };
    }

    public static Matrix4x4 ReadMatrix4x4(this PrimitiveReader reader)
    {
        return reader.ReadMatrix4x4(reader.ByteOrder);
    }

    public static Matrix4x4 ReadMatrix4x4(this PrimitiveReader reader, ByteOrder byteOrder)
    {
        return new Matrix4x4
        {
            M11 = reader.ReadSingle(byteOrder),
            M12 = reader.ReadSingle(byteOrder),
            M13 = reader.ReadSingle(byteOrder),
            M14 = reader.ReadSingle(byteOrder),

            M21 = reader.ReadSingle(byteOrder),
            M22 = reader.ReadSingle(byteOrder),
            M23 = reader.ReadSingle(byteOrder),
            M24 = reader.ReadSingle(byteOrder),

            M31 = reader.ReadSingle(byteOrder),
            M32 = reader.ReadSingle(byteOrder),
            M33 = reader.ReadSingle(byteOrder),
            M34 = reader.ReadSingle(byteOrder),

            M41 = reader.ReadSingle(byteOrder),
            M42 = reader.ReadSingle(byteOrder),
            M43 = reader.ReadSingle(byteOrder),
            M44 = reader.ReadSingle(byteOrder),
        };
    }
    
    public static Matrix4x4 ReadMatrix3x3(this PrimitiveReader reader)
    {
        return reader.ReadMatrix3x3(reader.ByteOrder);
    }

    public static Matrix4x4 ReadMatrix3x3(this PrimitiveReader reader, ByteOrder byteOrder)
    {
        return new Matrix4x4
        {
            M11 = reader.ReadSingle(byteOrder),
            M12 = reader.ReadSingle(byteOrder),
            M13 = reader.ReadSingle(byteOrder),

            M21 = reader.ReadSingle(byteOrder),
            M22 = reader.ReadSingle(byteOrder),
            M23 = reader.ReadSingle(byteOrder),

            M31 = reader.ReadSingle(byteOrder),
            M32 = reader.ReadSingle(byteOrder),
            M33 = reader.ReadSingle(byteOrder),

            M14 = 0.0f,
            M24 = 0.0f,
            M34 = 0.0f,
            M44 = 1.0f,

            M41 = 0.0f,
            M42 = 0.0f,
            M43 = 0.0f,
        };
    }

    public static T Deserialize<T>(this PrimitiveReader reader, SerializerBase<T> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        return serializer.Deserialize(reader);
    }

    public static ValueTask<T> DeserializeAsync<T>(this PrimitiveReader reader, SerializerBase<T> serializer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        return serializer.DeserializeAsync(reader, cancellationToken);
    }
}