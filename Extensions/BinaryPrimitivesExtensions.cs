using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using VT2AssetLib.IO;

namespace VT2AssetLib.Extensions;

internal static class BinaryPrimitivesExtensions
{
    public static short ReadInt16(ReadOnlySpan<byte> source, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        return byteOrder == ByteOrder.LittleEndian
            ? BinaryPrimitives.ReadInt16LittleEndian(source)
            : BinaryPrimitives.ReadInt16BigEndian(source);
    }

    public static ushort ReadUInt16(ReadOnlySpan<byte> source, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        return byteOrder == ByteOrder.LittleEndian
            ? BinaryPrimitives.ReadUInt16LittleEndian(source)
            : BinaryPrimitives.ReadUInt16BigEndian(source);
    }

    public static int ReadInt32(ReadOnlySpan<byte> source, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        return byteOrder == ByteOrder.LittleEndian
            ? BinaryPrimitives.ReadInt32LittleEndian(source)
            : BinaryPrimitives.ReadInt32BigEndian(source);
    }

    public static uint ReadUInt32(ReadOnlySpan<byte> source, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        return byteOrder == ByteOrder.LittleEndian
            ? BinaryPrimitives.ReadUInt32LittleEndian(source)
            : BinaryPrimitives.ReadUInt32BigEndian(source);
    }

    public static long ReadInt64(ReadOnlySpan<byte> source, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        return byteOrder == ByteOrder.LittleEndian
            ? BinaryPrimitives.ReadInt64LittleEndian(source)
            : BinaryPrimitives.ReadInt64BigEndian(source);
    }

    public static ulong ReadUInt64(ReadOnlySpan<byte> source, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        return byteOrder == ByteOrder.LittleEndian
            ? BinaryPrimitives.ReadUInt64LittleEndian(source)
            : BinaryPrimitives.ReadUInt64BigEndian(source);
    }

    public static Half ReadHalf(ReadOnlySpan<byte> source, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        return byteOrder == ByteOrder.LittleEndian
            ? BinaryPrimitives.ReadHalfLittleEndian(source)
            : BinaryPrimitives.ReadHalfBigEndian(source);
    }

    public static float ReadSingle(ReadOnlySpan<byte> source, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        return byteOrder == ByteOrder.LittleEndian
            ? BinaryPrimitives.ReadSingleLittleEndian(source)
            : BinaryPrimitives.ReadSingleBigEndian(source);
    }

    public static double ReadDouble(ReadOnlySpan<byte> source, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        return byteOrder == ByteOrder.LittleEndian
            ? BinaryPrimitives.ReadDoubleLittleEndian(source)
            : BinaryPrimitives.ReadDoubleBigEndian(source);
    }

    public static void WriteInt16(Span<byte> destination, short value, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        if (byteOrder == ByteOrder.LittleEndian)
            BinaryPrimitives.WriteInt16LittleEndian(destination, value);
        else
            BinaryPrimitives.WriteInt16BigEndian(destination, value);
    }

    public static void WriteUInt16(Span<byte> destination, ushort value, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        if (byteOrder == ByteOrder.LittleEndian)
            BinaryPrimitives.WriteUInt16LittleEndian(destination, value);
        else
            BinaryPrimitives.WriteUInt16BigEndian(destination, value);
    }

    public static void WriteInt32(Span<byte> destination, int value, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        if (byteOrder == ByteOrder.LittleEndian)
            BinaryPrimitives.WriteInt32LittleEndian(destination, value);
        else
            BinaryPrimitives.WriteInt32BigEndian(destination, value);
    }

    public static void WriteUInt32(Span<byte> destination, uint value, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        if (byteOrder == ByteOrder.LittleEndian)
            BinaryPrimitives.WriteUInt32LittleEndian(destination, value);
        else
            BinaryPrimitives.WriteUInt32BigEndian(destination, value);
    }

    public static void WriteInt64(Span<byte> destination, long value, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        if (byteOrder == ByteOrder.LittleEndian)
            BinaryPrimitives.WriteInt64LittleEndian(destination, value);
        else
            BinaryPrimitives.WriteInt64BigEndian(destination, value);
    }

    public static void WriteUInt64(Span<byte> destination, ulong value, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        if (byteOrder == ByteOrder.LittleEndian)
            BinaryPrimitives.WriteUInt64LittleEndian(destination, value);
        else
            BinaryPrimitives.WriteUInt64BigEndian(destination, value);
    }

    public static void WriteHalf(Span<byte> destination, Half value, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        if (byteOrder == ByteOrder.LittleEndian)
            BinaryPrimitives.WriteHalfLittleEndian(destination, value);
        else
            BinaryPrimitives.WriteHalfBigEndian(destination, value);
    }

    public static void WriteSingle(Span<byte> destination, float value, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        if (byteOrder == ByteOrder.LittleEndian)
            BinaryPrimitives.WriteSingleLittleEndian(destination, value);
        else
            BinaryPrimitives.WriteSingleBigEndian(destination, value);
    }

    public static void WriteDouble(Span<byte> destination, double value, ByteOrder byteOrder)
    {
        ThrowOnInvalidByteOrder(byteOrder);
        if (byteOrder == ByteOrder.LittleEndian)
            BinaryPrimitives.WriteDoubleLittleEndian(destination, value);
        else
            BinaryPrimitives.WriteDoubleBigEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowOnInvalidByteOrder(ByteOrder byteOrder)
    {
        if (byteOrder is not ByteOrder.LittleEndian and not ByteOrder.BigEndian)
            throw new ArgumentOutOfRangeException(nameof(byteOrder), "Byte order must be little endian or big endian.");
    }
}