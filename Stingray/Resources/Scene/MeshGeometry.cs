namespace VT2AssetLib.Stingray.Resources.Scene;

internal class MeshGeometry
{
    public VertexBuffer[] VertexBuffers { get; set; } = null!;

    public IndexBuffer IndexBuffer { get; set; } = null!;

    public BatchRange[] BatchRanges { get; set; } = null!;

    public BoundingVolume BoundingVolume { get; set; }

    public IDString32[] Materials { get; set; } = null!;
}