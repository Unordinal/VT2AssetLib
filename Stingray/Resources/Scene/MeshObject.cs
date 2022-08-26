namespace VT2AssetLib.Stingray.Resources.Scene;

internal sealed class MeshObject
{
    public IDString32 Name { get; set; }

    public int NodeIndex { get; set; }

    public uint GeometryIndex { get; set; }

    public uint SkinIndex { get; set; }

    public RenderableFlags Flags { get; set; }

    public BoundingVolume BoundingVolume { get; set; }
}