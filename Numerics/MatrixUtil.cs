using System.Numerics;

namespace VT2AssetLib.Numerics;

public static class MatrixUtil
{
    public static Matrix4x4 GetTRSMatrix(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        return GetTRSMatrix(position, Matrix4x4.CreateFromQuaternion(rotation), scale);
    }

    public static Matrix4x4 GetTRSMatrix(Vector3 position, Matrix4x4 rotation, Vector3 scale)
    {
        Matrix4x4 t = Matrix4x4.CreateTranslation(position);
        Matrix4x4 r = rotation;
        Matrix4x4 s = Matrix4x4.CreateScale(scale);

        return s * r * t;
    }
}