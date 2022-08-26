using System.Buffers;
using System.Collections;
using System.Diagnostics;

namespace VT2AssetLib.Collections;

internal class ChunkedArray<T> : IEnumerable<T>
{
    public static ChunkedArray<T> Empty { get; } = new();

    public ref T this[long index]
    {
        get
        {
            if (index < 0 || index > _length)
                throw new ArgumentOutOfRangeException(nameof(index));

            (int chunkIndex, int elementIndex) = Get2DIndexFrom1D(index);
            return ref _chunks[chunkIndex][elementIndex];
        }
    }

    public long Length => _length;

    private readonly T[][] _chunks;
    private readonly int _chunkSize;
    private readonly long _length;

    private ChunkedArray()
    {
        _chunks = Array.Empty<T[]>();
        _chunkSize = 0;
    }

    private ChunkedArray(T[][] chunks, int chunkSize)
    {
        _chunks = chunks;
        _chunkSize = chunkSize;
    }

    public ChunkedArray(long length, int chunkSize)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));
        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize));

        long chunkCount = GetChunkCountForLengthAndSize(length, chunkSize);

        _chunks = new T[chunkCount][];
        for (int i = 0; i < chunkCount; i++)
            _chunks[i] = new T[chunkSize];

        _chunkSize = chunkSize;
    }

    public ReadOnlySpan<T[]> AsSpan()
    {
        return _chunks;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (long i = 0; i < _length; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static ChunkedArray<T> Rent(int minimumLength, int chunkSize)
    {
        long chunkCount = GetChunkCountForLengthAndSize(minimumLength, chunkSize);

        T[][] chunks = new T[chunkCount][];
        for (int i = 0; i < chunkCount; i++)
            chunks[i] = ArrayPool<T>.Shared.Rent(chunkSize);

        return new ChunkedArray<T>(chunks, chunkSize);
    }

    public static void Return(ChunkedArray<T> array, bool clearArray = false)
    {
        for (int i = 0; i < array._chunks.Length; i++)
        {
            var chunk = array._chunks[i];
            ArrayPool<T>.Shared.Return(chunk, clearArray);
        }
    }

    private (int ChunkIndex, int ElementIndex) Get2DIndexFrom1D(long index)
    {
        int chunkIndex = (int)(index / _chunkSize);
        int chunkSize = (int)(index / _chunkSize);
        return (chunkIndex, chunkSize);
    }

    private static long GetChunkCountForLengthAndSize(long totalLength, int chunkSize)
    {
        Debug.Assert(totalLength >= 0);
        Debug.Assert(chunkSize >= 0);

        if (totalLength == 0)
            return 0;

        return (int)(((totalLength - 1) / chunkSize) + 1);
    }
}