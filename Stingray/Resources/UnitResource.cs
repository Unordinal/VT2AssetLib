using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT2AssetLib.Stingray.Resources.Scene;

namespace VT2AssetLib.Stingray.Resources;

internal sealed class UnitResource : IResource
{
    public uint Version { get; set; }

    public MeshGeometry[] MeshGeometries { get; set; } = null!;

    public SkinData[] Skins { get; set; } = null!;

    public byte[] SimpleAnimation { get; set; } = null!;

    public SimpleAnimationGroup[] SimpleAnimationGroups { get; set; } = null!;

    public SceneGraph SceneGraph { get; set; } = null!;

    public MeshObject[] Meshes { get; set; } = null!;

    public bool HasGeometry(MeshObject meshObject)
    {
        return meshObject.GeometryIndex > 0;
    }

    public bool HasSkin(MeshObject meshObject)
    {
        return meshObject.SkinIndex > 0;
    }

    public MeshGeometry GetGeometry(MeshObject meshObject)
    {
        if (!HasGeometry(meshObject))
            throw new ArgumentOutOfRangeException(nameof(meshObject), "No geometry exists for the mesh object.");

        return MeshGeometries[meshObject.GeometryIndex - 1];
    }

    public SkinData GetSkin(MeshObject meshObject)
    {
        if (!HasSkin(meshObject))
            throw new ArgumentOutOfRangeException(nameof(meshObject), "No skin data exists for the mesh object.");

        uint skinIndex = meshObject.SkinIndex - (uint)MeshGeometries.Length;
        return Skins[skinIndex - 1];
    }
}
