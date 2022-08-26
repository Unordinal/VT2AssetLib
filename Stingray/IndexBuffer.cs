namespace VT2AssetLib.Stingray;

internal sealed class IndexBuffer
{
    public Validity Validity { get; set; }

    public StreamType StreamType { get; set; }

    public IndexFormat IndexFormat { get; set; }

    public uint IndexCount { get; set; }

    public byte[] Data { get; set; } = null!;

    public override string ToString()
    {
        return $"{nameof(IndexBuffer)}<{IndexFormat}>[{IndexCount}]";
    }
}