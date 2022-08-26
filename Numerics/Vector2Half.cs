using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2AssetLib.Numerics;

public partial struct Vector2Half : IEquatable<Vector2Half>
{
    public Half X;
    public Half Y;

    public Vector2Half(Half value) : this(value, value)
    {
    }

    public Vector2Half(Half x, Half y)
    {
        X = x;
        Y = y;
    }

    public Vector2Half(ReadOnlySpan<Half> values)
    {
        if (values.Length < 2)
            throw new ArgumentOutOfRangeException(nameof(values));

        this = Unsafe.ReadUnaligned<Vector2Half>(ref Unsafe.As<Half, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2Half half && Equals(half);
    }

    public bool Equals(Vector2Half other)
    {
        return X == other.X &&
               Y == other.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"<{nameof(Vector2Half)} ({X}, {Y})>";
    }

    public static bool operator ==(Vector2Half left, Vector2Half right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2Half left, Vector2Half right)
    {
        return !(left == right);
    }
}

public partial struct Vector2Half
{
    public static Vector2Half Zero => default;

    public static Vector2Half One => new((Half)1.0f);

    public static Vector2Half UnitX => new((Half)1.0f, (Half)0.0f);

    public static Vector2Half UnitY => new((Half)0.0f, (Half)1.0f);

    public static implicit operator Vector3Half(Vector2Half value) => new(value.X, value.Y, (Half)0.0f);

    public static implicit operator Vector4Half(Vector2Half value) => new(value.X, value.Y, (Half)0.0f, (Half)0.0f);

    public static implicit operator Vector2(Vector2Half value) => new((float)value.X, (float)value.Y);

    public static explicit operator Vector2Half(Vector2 value) => new((Half)value.X, (Half)value.Y);
}