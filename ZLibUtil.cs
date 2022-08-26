using ICSharpCode.SharpZipLib.Zip.Compression;
using System.IO.Compression;
using VT2AssetLib.Collections;
using VT2AssetLib.IO.Extensions;

namespace VT2AssetLib;

internal static class ZLibUtil
{
    public static int Decompress(Inflater inflater, byte[] input, int start, int count, Span<byte> output)
    {
        inflater.SetInput(input, start, count);

        using RentedArray<byte> buffer = new(output.Length);
        int totalBytesDecompressed = 0;
        while (!inflater.IsFinished)
        {
            int bytesDecompressed = inflater.Inflate(buffer.Array, 0, output.Length);
            totalBytesDecompressed += bytesDecompressed;
            if (bytesDecompressed > output.Length)
                throw new ArgumentException($"The output span is not big enough to contain the decompressed bytes ({totalBytesDecompressed}).");

            buffer.AsSpan(0, bytesDecompressed).CopyTo(output);
            if (bytesDecompressed < output.Length)
                output = output[bytesDecompressed..];
        }

        return totalBytesDecompressed;
    }

    public static unsafe int Decompress(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        fixed (byte* pBuffer = source)
        {
            using var stream = new UnmanagedMemoryStream(pBuffer, source.Length);
            using var zlibStream = new ZLibStream(stream, CompressionMode.Decompress);

            return zlibStream.ReadExactly(destination);
        }
    }

    public static unsafe ValueTask<int> DecompressAsync(ReadOnlyMemory<byte> source, Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        fixed (byte* pBuffer = source.Span)
        {
            using var stream = new UnmanagedMemoryStream(pBuffer, source.Length);
            using var zlibStream = new ZLibStream(stream, CompressionMode.Decompress);

            return zlibStream.ReadExactlyAsync(destination, true, cancellationToken);
        }
    }
}