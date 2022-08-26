using System.Diagnostics;
using VT2AssetLib.Collections;

namespace VT2AssetLib.Stingray.IO;

internal partial class SegmentDecompressorStream : Stream
{
    private readonly SegmentDecompressor _decompressor;
    private readonly int _maxSegmentsToBuffer;
    private readonly bool _leaveOpen;
    private bool _disposed;

    private readonly RentedArray<byte> _buffer;
    private int _readOffset;
    private int _readLength;

    public SegmentDecompressorStream(Stream stream, int maxSegmentLength, int maxSegmentsToBuffer, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("The given stream does not support reading.");
        if (maxSegmentLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSegmentLength));
        if (maxSegmentsToBuffer <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSegmentsToBuffer));

        _decompressor = new SegmentDecompressor(stream, maxSegmentLength, leaveOpen);
        _maxSegmentsToBuffer = maxSegmentsToBuffer;
        _leaveOpen = false; // We made SegmentDecompressor and when we dispose it it'll decide based on 'leaveOpen'.

        _buffer = new RentedArray<byte>(maxSegmentLength * maxSegmentsToBuffer);
    }

    public SegmentDecompressorStream(SegmentDecompressor decompressor, int maxSegmentsToBuffer, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(decompressor);
        if (maxSegmentsToBuffer <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSegmentsToBuffer));

        _decompressor = decompressor;
        _maxSegmentsToBuffer = maxSegmentsToBuffer;
        _leaveOpen = leaveOpen;

        _buffer = new RentedArray<byte>(decompressor.MaxSegmentLength * maxSegmentsToBuffer);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();
        ValidateBufferArguments(buffer, offset, count);

        return Read(buffer.AsSpan(offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        ThrowIfDisposed();

        int bytesFromBuffer = ReadFromBuffer(buffer);
        if (bytesFromBuffer == buffer.Length)
            return bytesFromBuffer;

        int bytesFromNewSegments = FillBuffer();
        if (bytesFromNewSegments == 0)
            return bytesFromBuffer;

        return ReadFromBuffer(buffer[bytesFromBuffer..]) + bytesFromBuffer;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        int bytesFromBuffer = ReadFromBuffer(buffer.Span);
        if (bytesFromBuffer == buffer.Length)
            return bytesFromBuffer;

        int bytesFromNewSegments = await FillBufferAsync(cancellationToken).ConfigureAwait(false);
        if (bytesFromNewSegments == 0)
            return bytesFromBuffer;

        return ReadFromBuffer(buffer.Span[bytesFromBuffer..]) + bytesFromBuffer;
    }

    private int ReadFromBuffer(Span<byte> destination)
    {
        int bytesToRead = Math.Min(_readLength - _readOffset, destination.Length);
        Debug.Assert(bytesToRead >= 0);

        if (bytesToRead > 0)
        {
            _buffer.AsSpan(_readOffset, bytesToRead).CopyTo(destination);
            _readOffset += bytesToRead;
        }

        return bytesToRead;
    }

    private int FillBuffer()
    {
        Debug.Assert(_readOffset == _readLength);

        int totalBytesRead = 0;
        for (int i = 0; i < _maxSegmentsToBuffer; i++)
        {
            int bytesRead = _decompressor.Decompress(_buffer.AsSpan()[totalBytesRead..]);
            if (bytesRead == 0)
                break;

            totalBytesRead += bytesRead;
        }

        _readOffset = 0;
        _readLength = totalBytesRead;
        return totalBytesRead;
    }

    private async ValueTask<int> FillBufferAsync(CancellationToken cancellationToken = default)
    {
        Debug.Assert(_readOffset == _readLength);

        int totalBytesRead = 0;
        for (int i = 0; i < _maxSegmentsToBuffer; i++)
        {
            var bufferSlice = _buffer.AsMemory()[totalBytesRead..];
            if (bufferSlice.Length < _decompressor.MaxSegmentLength)
                throw new InvalidDataException("The buffer slice is too small to contain the max segment length.");

            int bytesRead = await _decompressor.DecompressAsync(bufferSlice, cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
                break;

            totalBytesRead += bytesRead;
        }

        _readOffset = 0;
        _readLength = totalBytesRead;
        return totalBytesRead;
    }

    protected void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
}

// Other overrides
internal partial class SegmentDecompressorStream : Stream
{
    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanSeek => false;

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        base.Dispose(disposing);
        if (disposing && !_leaveOpen)
        {
            _decompressor.Dispose();
        }

        _disposed = true;
    }

    public override long Length => throw new NotSupportedException();

    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override void Flush()
    {
    }
}