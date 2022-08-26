using VT2AssetLib.Extensions;

namespace VT2AssetLib.IO;

public class PrimitiveWriter : IDisposable
{
    public Stream BaseStream => _stream;

    public ByteOrder ByteOrder => _byteOrder;

    protected Stream _stream;
    protected readonly ByteOrder _byteOrder;
    protected readonly bool _leaveOpen;
    private bool _disposed;

    public PrimitiveWriter(Stream stream, ByteOrder byteOrder, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite)
            throw new ArgumentException("The given stream does not support writing.");

        _stream = stream;
        _byteOrder = byteOrder;
        _leaveOpen = leaveOpen;
    }

    public virtual long Seek(long offset, SeekOrigin origin)
    {
        return _stream.Seek(offset, origin);
    }

    public virtual void Flush()
    {
        _stream.Flush();
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public virtual void Write(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();
        _stream.Write(buffer, offset, count);
    }

    public virtual void Write(ReadOnlySpan<byte> buffer)
    {
        ThrowIfDisposed();
        _stream.Write(buffer);
    }

    public virtual ValueTask WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return new ValueTask(_stream.WriteAsync(buffer, offset, count, cancellationToken));
    }

    public virtual ValueTask Write(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _stream.WriteAsync(buffer, cancellationToken);
    }

    public virtual void Write(byte value)
    {
        _stream.WriteByte(value);
    }

    public virtual void Write(sbyte value)
    {
        _stream.WriteByte((byte)value);
    }

    public void Write(short value)
    {
        Write(value, _byteOrder);
    }

    public virtual void Write(short value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(short)];
        BinaryPrimitivesExtensions.WriteInt16(buffer, value, byteOrder);
        Write(buffer);
    }
    
    public void Write(ushort value)
    {
        Write(value, _byteOrder);
    }

    public virtual void Write(ushort value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        BinaryPrimitivesExtensions.WriteUInt16(buffer, value, byteOrder);
        Write(buffer);
    }
    
    public void Write(int value)
    {
        Write(value, _byteOrder);
    }

    public virtual void Write(int value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitivesExtensions.WriteInt32(buffer, value, byteOrder);
        Write(buffer);
    }
    
    public void Write(uint value)
    {
        Write(value, _byteOrder);
    }

    public virtual void Write(uint value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];
        BinaryPrimitivesExtensions.WriteUInt32(buffer, value, byteOrder);
        Write(buffer);
    }
    
    public void Write(long value)
    {
        Write(value, _byteOrder);
    }

    public virtual void Write(long value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BinaryPrimitivesExtensions.WriteInt64(buffer, value, byteOrder);
        Write(buffer);
    }
    
    public void Write(ulong value)
    {
        Write(value, _byteOrder);
    }

    public virtual void Write(ulong value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        BinaryPrimitivesExtensions.WriteUInt64(buffer, value, byteOrder);
        Write(buffer);
    }
    
    public void Write(Half value)
    {
        Write(value, _byteOrder);
    }

    public virtual void Write(Half value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(short)];
        BinaryPrimitivesExtensions.WriteHalf(buffer, value, byteOrder);
        Write(buffer);
    }
    
    public void Write(float value)
    {
        Write(value, _byteOrder);
    }

    public virtual void Write(float value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(float)];
        BinaryPrimitivesExtensions.WriteSingle(buffer, value, byteOrder);
        Write(buffer);
    }
    
    public void Write(double value)
    {
        Write(value, _byteOrder);
    }

    public virtual void Write(double value, ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(double)];
        BinaryPrimitivesExtensions.WriteDouble(buffer, value, byteOrder);
        Write(buffer);
    }

    protected void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    protected static void ValidateBufferArguments(byte[] buffer!!, int offset, int count)
    {
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), "The specified offset cannot be negative.");

        if ((uint)count > buffer.Length - offset)
            throw new ArgumentOutOfRangeException(nameof(count), "The specified count exceeds the buffer length with the given offset.");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            if (_leaveOpen)
                _stream.Flush();
            else
                _stream.Dispose();
        }

        _disposed = true;
    }
}