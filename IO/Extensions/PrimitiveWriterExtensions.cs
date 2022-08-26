using System.Numerics;
using System.Text;
using VT2AssetLib.Serialization;

namespace VT2AssetLib.IO;

public static class PrimitiveWriterExtensions
{
    private const int MaxStackAlloc = 128;

    public static void WriteCString(this PrimitiveWriter writer, string value)
    {
        using StackAllocHelper<byte> buffer = value.Length <= MaxStackAlloc
            ? new(stackalloc byte[value.Length])
            : new(value.Length);

        Encoding.ASCII.GetBytes(value, buffer.Span);
        writer.Write(buffer.Span);
        writer.Write(0);
    }

    public static void WritePString16(this PrimitiveWriter writer, string value)
    {
        writer.WritePString16(value, writer.ByteOrder);
    }

    public static void WritePString16(this PrimitiveWriter writer, string value, ByteOrder byteOrder)
    {
        writer.Write((short)value.Length, byteOrder);

        using StackAllocHelper<byte> buffer = value.Length <= MaxStackAlloc
            ? new(stackalloc byte[value.Length])
            : new(value.Length);

        Encoding.ASCII.GetBytes(value, buffer.Span);
        writer.Write(buffer.Span);
    }

    public static void WritePString32(this PrimitiveWriter writer, string value)
    {
        writer.WritePString32(value, writer.ByteOrder);
    }

    public static void WritePString32(this PrimitiveWriter writer, string value, ByteOrder byteOrder)
    {
        writer.Write(value.Length, byteOrder);

        using StackAllocHelper<byte> buffer = value.Length <= MaxStackAlloc
            ? new(stackalloc byte[value.Length])
            : new(value.Length);

        Encoding.ASCII.GetBytes(value, buffer.Span);
        writer.Write(buffer.Span);
    }

    public static void Write(this PrimitiveWriter writer, Vector2 value)
    {
        writer.Write(value, writer.ByteOrder);
    }

    public static void Write(this PrimitiveWriter writer, Vector2 value, ByteOrder byteOrder)
    {
        writer.Write(value.X, byteOrder);
        writer.Write(value.Y, byteOrder);
    }

    public static void Write(this PrimitiveWriter writer, Vector3 value)
    {
        writer.Write(value, writer.ByteOrder);
    }

    public static void Write(this PrimitiveWriter writer, Vector3 value, ByteOrder byteOrder)
    {
        writer.Write(value.X, byteOrder);
        writer.Write(value.Y, byteOrder);
        writer.Write(value.Z, byteOrder);
    }

    public static void Write(this PrimitiveWriter writer, Vector4 value)
    {
        writer.Write(value, writer.ByteOrder);
    }

    public static void Write(this PrimitiveWriter writer, Vector4 value, ByteOrder byteOrder)
    {
        writer.Write(value.X, byteOrder);
        writer.Write(value.Y, byteOrder);
        writer.Write(value.Z, byteOrder);
        writer.Write(value.W, byteOrder);
    }

    public static void Write(this PrimitiveWriter writer, Matrix4x4 value)
    {
        writer.Write(value, writer.ByteOrder);
    }

    public static void Write(this PrimitiveWriter writer, Matrix4x4 value, ByteOrder byteOrder)
    {
        writer.Write(value.M11, byteOrder);
        writer.Write(value.M12, byteOrder);
        writer.Write(value.M13, byteOrder);
        writer.Write(value.M14, byteOrder);

        writer.Write(value.M21, byteOrder);
        writer.Write(value.M22, byteOrder);
        writer.Write(value.M23, byteOrder);
        writer.Write(value.M24, byteOrder);

        writer.Write(value.M31, byteOrder);
        writer.Write(value.M32, byteOrder);
        writer.Write(value.M33, byteOrder);
        writer.Write(value.M34, byteOrder);

        writer.Write(value.M41, byteOrder);
        writer.Write(value.M42, byteOrder);
        writer.Write(value.M43, byteOrder);
        writer.Write(value.M44, byteOrder);
    }

    public static void Serialize<T>(this PrimitiveWriter writer, SerializerBase<T> serializer, T value)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        serializer.Serialize(writer, value);
    }

    public static ValueTask SerializeAsync<T>(this PrimitiveWriter writer, SerializerBase<T> serializer, T value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        return serializer.SerializeAsync(writer, value, cancellationToken);
    }
}