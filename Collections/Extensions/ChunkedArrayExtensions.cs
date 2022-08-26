namespace VT2AssetLib.Collections.Extensions;

internal static class ChunkedArrayExtensions
{
    public static void CopyTo<T>(this Span<T> source, ChunkedArray<T> destination)
    {
        source.CopyTo(destination, 0);
    }

    public static void CopyTo<T>(this Span<T> source, ChunkedArray<T> destination, long dstOffset)
    {
        if (source.Length > destination.Length - dstOffset)
            throw new ArgumentOutOfRangeException(nameof(destination), "The destination is too small to contain the source span.");

        for (int i = 0; i < source.Length; i++)
            destination[i + dstOffset] = source[i];
    }

    public static void CopyTo<T>(this ChunkedArray<T> source, Span<T> destination)
    {
        source.CopyTo(0, source.Length, destination);
    }

    public static void CopyTo<T>(this ChunkedArray<T> source, long srcOffset, long srcLength, Span<T> destination)
    {
        if ((ulong)srcOffset > (ulong)source.Length)
            throw new ArgumentOutOfRangeException(nameof(srcOffset));
        if ((ulong)srcLength > (ulong)(source.Length - srcOffset))
            throw new ArgumentOutOfRangeException(nameof(srcLength));
        if (destination.Length < srcLength)
            throw new ArgumentOutOfRangeException(nameof(destination), "The destination is too small to contain the source array.");

        for (int i = 0; i < srcLength; i++)
            destination[i] = source[i + srcOffset];
    }
}