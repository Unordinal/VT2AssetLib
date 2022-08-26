using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Compression;
using VT2AssetLib.Collections;
using VT2AssetLib.IO.Extensions;

namespace VT2AssetLib.Stingray.IO;

internal class SegmentDecompressor : IDisposable, IAsyncDisposable
{
    public int MaxSegmentLength => _maxSegmentLength;

    private readonly Stream _stream;
    private readonly int _maxSegmentLength;
    private readonly bool _leaveOpen;
    private readonly RentedArray<byte> _compressedBuffer;
    private bool _disposed;

    public SegmentDecompressor(Stream stream, int maxSegmentLength, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("The given stream does not support reading.", nameof(stream));
        if (maxSegmentLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSegmentLength));

        _stream = stream;
        _maxSegmentLength = maxSegmentLength;
        _leaveOpen = leaveOpen;
        _compressedBuffer = new RentedArray<byte>(maxSegmentLength);
    }

    public int Decompress(Span<byte> destination)
    {
        if (destination.Length < _maxSegmentLength)
            throw new ArgumentOutOfRangeException(nameof(destination), "Destination is not big enough to hold the max decompressed segment size.");

        if (!TryReadSegmentLength(out int segmentLength))
            return 0;

        var destSlice = destination[.._maxSegmentLength];
        if (segmentLength == _maxSegmentLength) // No decompression needed.
        {
            return _stream.ReadExactly(destSlice);
        }
        else
        {
            var compressedBuffer = _compressedBuffer.AsSpan()[..segmentLength];
            _stream.ReadExactly(compressedBuffer);
            return ZLibUtil.Decompress(compressedBuffer, destSlice);
        }
    }

    public async ValueTask<int> DecompressAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        if (destination.Length < _maxSegmentLength)
            throw new ArgumentOutOfRangeException(nameof(destination), "Destination is not big enough to hold the max decompressed segment size.");

        if (!TryReadSegmentLength(out int segmentLength))
            return 0;

        var destSlice = destination[.._maxSegmentLength];
        if (segmentLength == _maxSegmentLength) // No decompression needed.
        {
            return await _stream.ReadExactlyAsync(destSlice, true, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var compressedBuffer = _compressedBuffer.AsMemory()[..segmentLength];

            await _stream.ReadExactlyAsync(compressedBuffer, true, cancellationToken).ConfigureAwait(false);
            return await ZLibUtil.DecompressAsync(compressedBuffer, destSlice, cancellationToken).ConfigureAwait(false);
        }
    }

    private bool TryReadSegmentLength(out int segmentLength)
    {
        Span<byte> segmentLengthBytes = stackalloc byte[4];
        int bytesRead = _stream.ReadExactly(segmentLengthBytes, false);
        if (bytesRead < 4)
        {
            segmentLength = 0;
            return false;
        }

        segmentLength = BinaryPrimitives.ReadInt32LittleEndian(segmentLengthBytes);
        if (segmentLength <= 0)
            throw new InvalidDataException("The read segment length is less than or equal to zero.");
        if (segmentLength > _maxSegmentLength)
            throw new InvalidDataException("The read segment size is greater than the max segment size.");

        return true;
    }

    #region Disposal

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _compressedBuffer.Dispose();

            if (!_leaveOpen)
                _stream.Dispose();
        }

        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(disposing: true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _compressedBuffer.Dispose();

            if (!_leaveOpen)
                await _stream.DisposeAsync().ConfigureAwait(false);
        }

        _disposed = true;
    }

    #endregion Disposal
}