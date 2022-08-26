using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VT2AssetLib.Extensions;

namespace VT2AssetLib.IO;

public class PrimitiveReader : IDisposable
{
    public Stream BaseStream => _stream;

    public ByteOrder ByteOrder => _byteOrder;

    protected readonly Stream _stream;
    protected readonly ByteOrder _byteOrder;
    protected readonly bool _leaveOpen;
    private bool _disposed;

    public PrimitiveReader(Stream stream, ByteOrder byteOrder, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("The given stream does not support reading.");

        _stream = stream;
        _byteOrder = byteOrder;
        _leaveOpen = leaveOpen;
    }

    public virtual long Seek(long offset, SeekOrigin origin)
    {
        return _stream.Seek(offset, origin);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public virtual int Read(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();
        return _stream.Read(buffer, offset, count);
    }

    public virtual int Read(Span<byte> buffer)
    {
        ThrowIfDisposed();
        return _stream.Read(buffer);
    }

    public virtual ValueTask<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        return _stream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
    }

    public virtual ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _stream.ReadAsync(buffer, cancellationToken);
    }

    public int ReadExactly(byte[] buffer, int offset, int count, bool throwOnEndOfStream = true)
    {
        ValidateBufferArguments(buffer, offset, count);
        return ReadExactly(buffer.AsSpan(offset, count), throwOnEndOfStream);
    }

    public int ReadExactly(Span<byte> buffer, bool throwOnEndOfStream = true)
    {
        int count = buffer.Length;
        int totalBytesRead = 0;
        while (totalBytesRead < count)
        {
            int bytesRead = Read(buffer[totalBytesRead..]);
            if (bytesRead == 0)
            {
                if (throwOnEndOfStream)
                    throw new EndOfStreamException();

                break;
            }

            totalBytesRead += bytesRead;
        }

        return totalBytesRead;
    }

    public ValueTask<int> ReadExactlyAsync(byte[] buffer, int offset, int count, bool throwOnEndOfStream = true, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ValidateBufferArguments(buffer, offset, count);

        return ReadExactlyAsync(buffer.AsMemory(offset, count), throwOnEndOfStream, cancellationToken);
    }

    public async ValueTask<int> ReadExactlyAsync(Memory<byte> buffer, bool throwOnEndOfStream = true, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        int totalBytesRead = 0;
        while (totalBytesRead < buffer.Length)
        {
            int bytesRead = await ReadAsync(buffer[totalBytesRead..], cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
            {
                if (throwOnEndOfStream)
                    throw new EndOfStreamException();

                break;
            }

            totalBytesRead += bytesRead;
        }

        return totalBytesRead;
    }

    public byte[] ReadBytes(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "The specified count cannot be negative.");
        ThrowIfDisposed();
        if (count == 0)
            return Array.Empty<byte>();

        byte[] result = new byte[count];
        int bytesRead = ReadExactly(result, false);

        if (bytesRead < result.Length)
        {
            byte[] copy = new byte[bytesRead];
            Buffer.BlockCopy(result, 0, copy, 0, bytesRead);
            result = copy;
        }

        return result;
    }

    public async Task<byte[]> ReadBytesAsync(int count, CancellationToken cancellationToken = default)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "The specified count cannot be negative.");
        ThrowIfDisposed();
        if (count == 0)
            return Array.Empty<byte>();

        byte[] result = new byte[count];
        int bytesRead = await ReadExactlyAsync(result, false, cancellationToken).ConfigureAwait(false);

        if (bytesRead < result.Length)
        {
            byte[] copy = new byte[bytesRead];
            Buffer.BlockCopy(result, 0, copy, 0, bytesRead);
            result = copy;
        }

        return result;
    }

    public virtual byte ReadByte()
    {
        int result = _stream.ReadByte();
        if (result == -1)
            throw new EndOfStreamException();

        return (byte)result;
    }

    public virtual sbyte ReadSByte()
    {
        return (sbyte)ReadByte();
    }

    public short ReadInt16()
    {
        return ReadInt16(_byteOrder);
    }

    public virtual short ReadInt16(ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(short)];
        ReadExactly(buffer);
        return BinaryPrimitivesExtensions.ReadInt16(buffer, byteOrder);
    }

    public ushort ReadUInt16()
    {
        return ReadUInt16(_byteOrder);
    }

    public virtual ushort ReadUInt16(ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        ReadExactly(buffer);
        return BinaryPrimitivesExtensions.ReadUInt16(buffer, byteOrder);
    }

    public int ReadInt32()
    {
        return ReadInt32(_byteOrder);
    }

    public virtual int ReadInt32(ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        ReadExactly(buffer);
        return BinaryPrimitivesExtensions.ReadInt32(buffer, byteOrder);
    }

    public uint ReadUInt32()
    {
        return ReadUInt32(_byteOrder);
    }

    public virtual uint ReadUInt32(ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];
        ReadExactly(buffer);
        return BinaryPrimitivesExtensions.ReadUInt32(buffer, byteOrder);
    }

    public long ReadInt64()
    {
        return ReadInt64(_byteOrder);
    }

    public virtual long ReadInt64(ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        ReadExactly(buffer);
        return BinaryPrimitivesExtensions.ReadInt64(buffer, byteOrder);
    }

    public ulong ReadUInt64()
    {
        return ReadUInt64(_byteOrder);
    }

    public virtual ulong ReadUInt64(ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        ReadExactly(buffer);
        return BinaryPrimitivesExtensions.ReadUInt64(buffer, byteOrder);
    }

    public Half ReadHalf()
    {
        return ReadHalf(_byteOrder);
    }

    public virtual Half ReadHalf(ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(short)];
        ReadExactly(buffer);
        return BinaryPrimitivesExtensions.ReadHalf(buffer, byteOrder);
    }

    public float ReadSingle()
    {
        return ReadSingle(_byteOrder);
    }

    public virtual float ReadSingle(ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(float)];
        ReadExactly(buffer);
        return BinaryPrimitivesExtensions.ReadSingle(buffer, byteOrder);
    }

    public double ReadDouble()
    {
        return ReadDouble(_byteOrder);
    }

    public virtual double ReadDouble(ByteOrder byteOrder)
    {
        Span<byte> buffer = stackalloc byte[sizeof(double)];
        ReadExactly(buffer);
        return BinaryPrimitivesExtensions.ReadDouble(buffer, byteOrder);
    }

    public virtual T ReadStruct<T>() where T : unmanaged
    {
        int sizeOfT = Unsafe.SizeOf<T>();
        Span<byte> buffer = stackalloc byte[sizeOfT];
        ReadExactly(buffer);
        return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buffer));
    }

    public bool PeekByte(out byte value)
    {
        if (!_stream.CanSeek)
            throw new NotSupportedException("The given stream does not support seeking.");

        var currPos = _stream.Position;
        int result = _stream.ReadByte();
        bool success = result != -1;
        _stream.Position = currPos;

        value = (byte)(success ? result : 0);
        return success;
    }

    public bool PeekSByte(out sbyte value)
    {
        if (!_stream.CanSeek)
            throw new NotSupportedException("The given stream does not support seeking.");

        bool success = PeekByte(out byte result);
        value = (sbyte)result;
        return success;
    }

    public bool PeekBytes(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        long savedPos = _stream.Position;
        Span<byte> buffer = stackalloc byte[Math.Min(count, 256)];

        int totalBytesRead = 0;
        int bytesLeft = count;
        while (totalBytesRead < count)
        {
            int bytesToRead = Math.Min(bytesLeft, 256);
            int bytesRead = Read(buffer[..bytesToRead]);
            if (bytesRead == 0)
                break;

            bytesLeft -= bytesRead;
            totalBytesRead += bytesRead;
        }

        _stream.Position = savedPos;
        return totalBytesRead == count;
    }

    public T? Peek<T>(Func<PrimitiveReader, T> peekFunc)
    {
        ArgumentNullException.ThrowIfNull(peekFunc);
        var savedPos = BaseStream.Position;
        try
        {
            T readValue = peekFunc(this);
            return readValue;
        }
        catch
        {
            return default;
        }
        finally
        {
            BaseStream.Position = savedPos;
        }
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

        if (disposing && !_leaveOpen)
        {
            _stream.Dispose();
        }

        _disposed = true;
    }
}