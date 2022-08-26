namespace VT2AssetLib.Stingray;

internal sealed class VertexBuffer
{
    public Validity Validity { get; set; }

    public StreamType StreamType { get; set; }

    public uint Count { get; set; }

    public uint Stride { get; set; }

    public Channel Channel { get; set; } = null!;

    public byte[] Data { get; set; } = null!;

    public override string ToString()
    {
        return $"{Channel}[{Count}]";
    }
}