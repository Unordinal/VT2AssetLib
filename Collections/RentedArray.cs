using System.Buffers;
using System.Collections;
using System.Diagnostics;

namespace VT2AssetLib.Collections;

/// <summary>
/// Provides an easy way to automatically rent and return arrays using <see cref="ArrayPool{T}"/> with '<see langword="using"/>' statements.
/// </summary>
/// <typeparam name="T"></typeparam>
public struct RentedArray<T> : IEnumerable<T>, IDisposable
{
    public ref T this[int index]
    {
        get
        {
            ThrowIfDisposed();
            return ref _array[index];
        }
    }

    /// <summary>
    /// Returns the raw rented array. The length may exceed the originally requested minimum length.
    /// </summary>
    public T[] Array
    {
        get
        {
            ThrowIfDisposed();
            return _array;
        }
    }

    /// <summary>
    /// Returns a span over the rented array with exactly the originally requested length.
    /// </summary>
    public Span<T> Span
    {
        get
        {
            ThrowIfDisposed();
            return _array.AsSpan(.._length);
        }
    }

    /// <summary>
    /// Gets the originally requested length.
    /// </summary>
    public int Length => _length;

    private T[] _array;
    private readonly int _length;
    private readonly ArrayPool<T> _sourcePool;
    private readonly bool _clearOnReturn;
    private bool _disposed;

    public RentedArray(int minimumLength, ArrayPool<T>? sourcePool, bool clearOnReturn = false)
    {
        _sourcePool = sourcePool ?? ArrayPool<T>.Shared;
        _array = _sourcePool.Rent(minimumLength);
        _length = minimumLength;
        _clearOnReturn = clearOnReturn;
        _disposed = false;
    }

    public RentedArray(int minimumLength) : this(minimumLength, null, false)
    {
    }

    public Span<T> AsSpan()
    {
        return Span;
    }

    public Span<T> AsSpan(int start)
    {
        ThrowIfDisposed();
        return Span[start..];
    }

    public Span<T> AsSpan(int start, int length)
    {
        ThrowIfDisposed();
        return Span.Slice(start, length);
    }

    public Span<T> AsSpan(Index startIndex)
    {
        ThrowIfDisposed();
        return Span[startIndex..];
    }

    public Memory<T> AsMemory()
    {
        return _array.AsMemory(0, _length);
    }

    public Memory<T> AsMemory(int start)
    {
        return _array.AsMemory(start, _length - start);
    }

    public Memory<T> AsMemory(int start, int length)
    {
        if ((uint)start + (uint)length > (ulong)_length)
            throw new ArgumentOutOfRangeException(nameof(start));

        return _array.AsMemory(start, length);
    }

    public Memory<T> AsMemory(Index startIndex)
    {
        return _array.AsMemory(startIndex.GetOffset(_length));
    }

    public Memory<T> AsMemory(Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(_length);
        return _array.AsMemory(start, length);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _sourcePool.Return(Interlocked.Exchange(ref _array!, null), _clearOnReturn);
        _disposed = true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        ThrowIfDisposed();
        for (int i = 0; i < _length; i++)
            yield return _array[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        ThrowIfDisposed();
        return GetEnumerator();
    }

    [DebuggerStepThrough, DebuggerHidden]
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(typeof(T).FullName);
    }
}