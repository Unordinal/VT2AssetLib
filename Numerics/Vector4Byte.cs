using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VT2AssetLib.Numerics;

public partial struct Vector4Byte : IEquatable<Vector4Byte>
{
    public byte X;
    public byte Y;
    public byte Z;
    public byte W;

    public Vector4Byte(byte value) : this(value, value, value, value)
    {
    }

    public Vector4Byte(byte x, byte y, byte z, byte w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vector4Byte(ReadOnlySpan<byte> values)
    {
        if (values.Length < 4)
            throw new ArgumentOutOfRangeException(nameof(values));

        this = Unsafe.ReadUnaligned<Vector4Byte>(ref MemoryMarshal.GetReference(values));
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector4Byte vec4Byte && Equals(vec4Byte);
    }

    public bool Equals(Vector4Byte other)
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
        return $"<{nameof(Vector4Byte)} ({X}, {Y}, {Z}, {W})>";
    }

    public static bool operator ==(Vector4Byte left, Vector4Byte right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector4Byte left, Vector4Byte right)
    {
        return !(left == right);
    }
}

public partial struct Vector4Byte
{
    public static Vector4Byte Zero => default;

    public static Vector4Byte One => new(1);

    public static Vector4Byte UnitX => new(1, 0, 0, 0);

    public static Vector4Byte UnitY => new(0, 1, 0, 0);

    public static Vector4Byte UnitZ => new(0, 0, 1, 0);

    public static Vector4Byte UnitW => new(0, 0, 0, 1);
}