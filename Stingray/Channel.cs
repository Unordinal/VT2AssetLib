namespace VT2AssetLib.Stingray;

internal class Channel
{
    public VertexComponent Component { get; set; }

    public ChannelType Type { get; set; }

    public uint Set { get; set; } // TEXCOORD_0, TEXCOORD_1, etc

    public uint Stream { get; set; }

    public byte IsInstance { get; set; }

    public override string ToString()
    {
        return $"{GetComponentWithSet()}<{Type.ToString().ToLowerInvariant()}>";
    }

    private string GetComponentWithSet()
    {
        switch (Component)
        {
            case VertexComponent.Texcoord:
            case VertexComponent.Color:
            case VertexComponent.BlendIndices:
            case VertexComponent.BlendWeights:
                return $"{Component}_{Set}".ToUpperInvariant();

            default:
                return Component.ToString().ToUpperInvariant();
        }
    }
}