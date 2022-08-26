using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace VT2AssetLib;

[SkipLocalsInit]
internal readonly ref struct StackAllocHelper<T>
{
    public Span<T> Span => _span;

    private readonly Span<T> _span;
    private readonly ArrayPool<T>? _sourcePool;
    private readonly T[]? _rented;

    public StackAllocHelper(Span<T> buffer)
    {
        _span = buffer;
        _sourcePool = null;
        Unsafe.SkipInit(out _rented);
    }

    public StackAllocHelper(int length) : this(ArrayPool<T>.Shared, length)
    {
    }

    public StackAllocHelper(ArrayPool<T> sourcePool!!, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        _sourcePool = sourcePool;
        _rented = sourcePool.Rent(length);
        _span = _rented.AsSpan(0, length);
    }

    public void Dispose()
    {
        if (_sourcePool is not null)
        {
            Debug.Assert(_rented is not null);
            _sourcePool.Return(_rented);
        }
    }
}