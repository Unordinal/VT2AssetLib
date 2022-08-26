using System.Numerics;

namespace VT2AssetLib.Stingray.Resources.Scene;

internal class SceneGraphData
{
    public Matrix4x4 LocalTransform { get; set; }

    public Matrix4x4 WorldTransform { get; set; }

    public ParentType ParentType { get; set; }

    public ushort ParentIndex { get; set; }

    public IDString32 Name { get; set; }
}