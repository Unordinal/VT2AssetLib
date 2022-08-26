namespace VT2AssetLib.Extensions;

internal static class AssimpExtensions
{
    public static Assimp.Matrix4x4 ToAssimp(this System.Numerics.Matrix4x4 m, bool transpose = true)
    {
        if (transpose)
            m = System.Numerics.Matrix4x4.Transpose(m);

        return new Assimp.Matrix4x4
        (
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44
        );
    }

    public static Assimp.Vector3D ToAssimp(this System.Numerics.Vector3 v)
    {
        return new Assimp.Vector3D(v.X, v.Y, v.Z);
    }

    public static Assimp.Vector3D ToAssimp(this System.Numerics.Vector4 v)
    {
        return new Assimp.Vector3D(v.X, v.Y, v.Z);
    }

    public static Assimp.Quaternion ToAssimp(this System.Numerics.Quaternion v)
    {
        return new Assimp.Quaternion(v.W, v.X, v.Y, v.Z);
    }

    public static Assimp.Vector2D ToAssimp(this Numerics.Vector2Half v)
    {
        return new Assimp.Vector2D((float)v.X, (float)v.Y);
    }

    public static Assimp.Vector3D ToAssimp(this Numerics.Vector3Half v)
    {
        return new Assimp.Vector3D((float)v.X, (float)v.Y, (float)v.Z);
    }

    public static Assimp.Vector3D ToAssimp(this Numerics.Vector4Half v)
    {
        return new Assimp.Vector3D((float)v.X, (float)v.Y, (float)v.Z);
    }
}