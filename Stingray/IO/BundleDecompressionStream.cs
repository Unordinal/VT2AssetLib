using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using VT2AssetLib.Collections;
using VT2AssetLib.IO;

namespace VT2AssetLib.Stingray.IO;

internal partial class BundleDecompressionStream : Stream
{
    private const int MaxChunkSize = 0x10000;

    private PrimitiveReader? _reader;
    private Inflater? _inflater;
    private readonly int _chunksToBuffer;

    private RentedArray<byte>? _buffer;
    private int _readOffset;
    private int _readLength;

    public BundleDecompressionStream(Stream stream, int chunksToBuffer, bool leaveOpen = false)
        : this(new PrimitiveReader(stream, ByteOrder.LittleEndian, leaveOpen), chunksToBuffer)
    {
    }

    public BundleDecompressionStream(PrimitiveReader reader, int chunksToBuffer)
    {
        ArgumentNullException.ThrowIfNull(reader);
        if (chunksToBuffer < 1)
            throw new ArgumentOutOfRangeException(nameof(chunksToBuffer));

        _reader = reader;
        _chunksToBuffer = chunksToBuffer;
        Debug.Assert(MaxChunkSize * chunksToBuffer <= int.MaxValue);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();
        ValidateBufferArguments(buffer, offset, count);
        EnsureInitialized();

        return Read(buffer.AsSpan(offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        ThrowIfDisposed();
        EnsureInitialized();

        int bytesFromBuffer = ReadFromBuffer(buffer);
        if (bytesFromBuffer == buffer.Length)
            return bytesFromBuffer;

        if (bytesFromBuffer > 0)
            buffer = buffer[bytesFromBuffer..];

        int bytesFromNewChunks = FillBuffer();
        if (bytesFromNewChunks == 0)
            return bytesFromBuffer;

        return ReadFromBuffer(buffer) + bytesFromBuffer;
    }

    private int ReadFromBuffer(Span<byte> destination)
    {
        AssertInitializedAndNotDisposed();
        var buffer = _buffer.Value;
        int bytesToRead = Math.Min(_readLength - _readOffset, destination.Length);
        Debug.Assert(bytesToRead >= 0);

        if (bytesToRead > 0)
        {
            buffer.AsSpan(_readOffset, bytesToRead).CopyTo(destination);
            _readOffset += bytesToRead;
        }

        return bytesToRead;
    }

    private int FillBuffer()
    {
        AssertInitializedAndNotDisposed();
        Debug.Assert(_readOffset == _readLength);
        var buffer = _buffer.Value;

        int totalBytesRead = 0;
        for (int i = 0; i < _chunksToBuffer; i++)
        {
            var chunk = buffer.AsSpan(totalBytesRead);
            int bytesRead = ReadChunk(chunk);
            if (bytesRead == 0)
                break;

            totalBytesRead += bytesRead;
        }

        _readOffset = 0;
        _readLength = totalBytesRead;
        return totalBytesRead;
    }

    private int ReadChunk(Span<byte> destination)
    {
        AssertInitializedAndNotDisposed();
        Debug.Assert(destination.Length >= MaxChunkSize);
        if (!_reader.PeekBytes(4))
            return 0;

        int compressedSize = _reader.ReadInt32();
        Debug.Assert(destination.Length >= compressedSize && compressedSize > 0);

        if (compressedSize < MaxChunkSize) // Chunk is compressed.
        {
            using RentedArray<byte> compressed = new(compressedSize);
            _reader.ReadExactly(compressed.Span);

            _inflater.Reset();
            return ZLibUtil.Decompress(_inflater, compressed.Array, 0, compressedSize, destination);
        }
        else // Chunk is already decompressed.
        {
            return _reader.ReadExactly(destination[..compressedSize]);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_reader is null)
            return;

        base.Dispose(disposing);
        if (disposing)
        {
            Interlocked.Exchange(ref _reader, null).Dispose();
            if (_buffer.HasValue)
                _buffer.Value.Dispose();
        }
    }

    [MemberNotNull(nameof(_inflater), nameof(_buffer))]
    private void EnsureInitialized()
    {
        Debug.Assert(_reader is not null);
        _inflater ??= new Inflater();
        _buffer ??= new RentedArray<byte>(MaxChunkSize * _chunksToBuffer);
    }

    [Conditional("DEBUG")]
    [MemberNotNull(nameof(_reader), nameof(_inflater), nameof(_buffer))]
    private void AssertInitializedAndNotDisposed()
    {
        Debug.Assert(_reader is not null);
        Debug.Assert(_inflater is not null);
        Debug.Assert(_buffer is not null);
    }

    [MemberNotNull(nameof(_reader))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_reader is null)
            throw new ObjectDisposedException(GetType().FullName);
    }
}

// Unsupported inherited members
internal partial class BundleDecompressionStream : Stream
{
    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanSeek => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override void Flush()
    {
    }
}