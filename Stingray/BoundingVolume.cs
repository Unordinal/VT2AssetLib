using System.Numerics;

namespace VT2AssetLib.Stingray;

public readonly struct BoundingVolume
{
    public Vector3 LowerBounds { get; }

    public Vector3 UpperBounds { get; }

    public float Radius { get; }

    public BoundingVolume(Vector3 lowerBounds, Vector3 upperBounds, float radius)
    {
        LowerBounds = lowerBounds;
        UpperBounds = upperBounds;
        Radius = radius;
    }
}