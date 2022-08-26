using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2AssetLib.Numerics;

public partial struct Vector3Half : IEquatable<Vector3Half>
{
    public Half X;
    public Half Y;
    public Half Z;

    public Vector3Half(Half value) : this(value, value, value)
    {
    }

    public Vector3Half(Half x, Half y, Half z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3Half(ReadOnlySpan<Half> values)
    {
        if (values.Length < 3)
            throw new ArgumentOutOfRangeException(nameof(values));

        this = Unsafe.ReadUnaligned<Vector3Half>(ref Unsafe.As<Half, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector3Half half && Equals(half);
    }

    public bool Equals(Vector3Half other)
    {
        return X == other.X &&
               Y == other.Y &&
               Z == other.Z;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public override string ToString()
    {
        return $"<{nameof(Vector3Half)} ({X}, {Y}, {Z})>";
    }

    public static bool operator ==(Vector3Half left, Vector3Half right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector3Half left, Vector3Half right)
    {
        return !(left == right);
    }
}

public partial struct Vector3Half
{
    public static Vector3Half Zero => default;

    public static Vector3Half One => new((Half)1.0f);

    public static Vector3Half UnitX => new((Half)1.0f, (Half)0.0f, (Half)0.0f);

    public static Vector3Half UnitY => new((Half)0.0f, (Half)1.0f, (Half)0.0f);

    public static Vector3Half UnitZ => new((Half)0.0f, (Half)0.0f, (Half)1.0f);

    public static explicit operator Vector2Half(Vector3Half value) => new(value.X, value.Y);

    public static implicit operator Vector4Half(Vector3Half value) => new(value.X, value.Y, value.Z, (Half)0.0f);

    public static implicit operator Vector3(Vector3Half value) => new((float)value.X, (float)value.Y, (float)value.Z);

    public static explicit operator Vector3Half(Vector3 value) => new((Half)value.X, (Half)value.Y, (Half)value.Z);
}