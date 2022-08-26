using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2AssetLib.Numerics;

public partial struct Vector4Half : IEquatable<Vector4Half>
{
    public Half X;
    public Half Y;
    public Half Z;
    public Half W;

    public Vector4Half(Half value) : this(value, value, value, value)
    {
    }

    public Vector4Half(Half x, Half y, Half z, Half w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vector4Half(ReadOnlySpan<Half> values)
    {
        if (values.Length < 4)
            throw new ArgumentOutOfRangeException(nameof(values));

        this = Unsafe.ReadUnaligned<Vector4Half>(ref Unsafe.As<Half, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector4Half half && Equals(half);
    }

    public bool Equals(Vector4Half other)
    {
        return X == other.X &&
               Y == other.Y &&
               Z == other.Z &&
               W == other.W;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
    }

    public override string ToString()
    {
        return $"<{nameof(Vector4Half)} ({X}, {Y}, {Z}, {W})>";
    }

    public static bool operator ==(Vector4Half left, Vector4Half right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector4Half left, Vector4Half right)
    {
        return !(left == right);
    }
}

public partial struct Vector4Half
{
    public static Vector4Half Zero => default;

    public static Vector4Half One => new((Half)1.0f);

    public static Vector4Half UnitX => new((Half)1.0f, (Half)0.0f, (Half)0.0f, (Half)0.0f);

    public static Vector4Half UnitY => new((Half)0.0f, (Half)1.0f, (Half)0.0f, (Half)0.0f);

    public static Vector4Half UnitZ => new((Half)0.0f, (Half)0.0f, (Half)1.0f, (Half)0.0f);

    public static Vector4Half UnitW => new((Half)0.0f, (Half)0.0f, (Half)0.0f, (Half)1.0f);

    public static explicit operator Vector2Half(Vector4Half value) => new(value.X, value.Y);

    public static explicit operator Vector3Half(Vector4Half value) => new(value.X, value.Y, value.Z);

    public static explicit operator Vector2(Vector4Half value) => new((float)value.X, (float)value.Y);

    public static explicit operator Vector3(Vector4Half value) => new((float)value.X, (float)value.Y, (float)value.Z);

    public static implicit operator Vector4(Vector4Half value) => new((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);

    public static explicit operator Vector4Half(Vector4 value) => new((Half)value.X, (Half)value.Y, (Half)value.Z, (Half)value.W);
}