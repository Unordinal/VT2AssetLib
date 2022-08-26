namespace VT2AssetLib.IO.Extensions;

internal static class StreamExtensions
{
    private static readonly byte[] _buffer = GC.AllocateArray<byte>(8192);

    public static int ReadExactly(this Stream stream, Span<byte> buffer, bool throwOnEndOfStream = true)
    {
        if (buffer.IsEmpty)
            return 0;

        int totalBytesRead = 0;
        while (totalBytesRead < buffer.Length)
        {
            int bytesRead = stream.Read(buffer[totalBytesRead..]);
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

    public static async ValueTask<int> ReadExactlyAsync(this Stream stream, Memory<byte> buffer, bool throwOnEndOfStream = true, CancellationToken cancellationToken = default)
    {
        if (buffer.IsEmpty)
            return 0;

        int totalBytesRead = 0;
        while (totalBytesRead < buffer.Length)
        {
            int bytesRead = await stream.ReadAsync(buffer[totalBytesRead..], cancellationToken).ConfigureAwait(false);
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

    public static int SkipBytes(this Stream stream, int count, bool throwOnEndOfStream = true)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (count == 0)
            return 0;

        if (stream.CanSeek)
        {
            stream.Seek(count, SeekOrigin.Current);
            return count;
        }

        int totalBytesRead = 0;
        while (totalBytesRead < count)
        {
            int bytesToRead = Math.Min(count - totalBytesRead, _buffer.Length);
            int bytesRead = stream.Read(_buffer, 0, bytesToRead);
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

    public static async ValueTask<int> SkipBytesAsync(this Stream stream, int count, bool throwOnEndOfStream = true, CancellationToken cancellationToken = default)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (count == 0)
            return 0;

        if (stream.CanSeek)
        {
            stream.Seek(count, SeekOrigin.Current);
            return count;
        }

        int totalBytesRead = 0;
        while (totalBytesRead < count)
        {
            int bytesToRead = Math.Min(count - totalBytesRead, _buffer.Length);
            int bytesRead = await stream.ReadAsync(_buffer, 0, bytesToRead, cancellationToken);
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
}