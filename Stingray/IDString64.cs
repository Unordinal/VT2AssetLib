namespace VT2AssetLib.Stingray;

/// <summary>
/// Represents a 64-bit Murmur2 hashed string value.
/// </summary>
public readonly struct IDString64 : IEquatable<IDString64>, IComparable<IDString64>
{
    /// <summary>
    /// Gets the empty string.
    /// </summary>
    public static IDString64 Empty { get; } = new(0, string.Empty);

    /// <summary>
    /// Gets the 64-bit hash of the original string value.
    /// </summary>
    public ulong ID { get; }

    /// <summary>
    /// Gets the original string value, if we know what it is.
    /// <para/>
    /// Consider using <see cref="ToString"/> instead of <c>Value ?? ID.ToString("x16")</c>.
    /// </summary>
    /// <returns>The original string value if we know it; otherwise, <see langword="null"/>.</returns>
    public string? Value { get; }

    /// <summary>
    /// Gets whether this is equal to <see cref="Empty"/> (has an ID of <c>0</c>).
    /// </summary>
    public bool IsEmpty => ID == 0;

    /// <summary>
    /// Creates a new <see cref="IDString64"/> using the specified hash and without a known string value.
    /// </summary>
    /// <param name="id">The hash of the string.</param>
    public IDString64(ulong id)
    {
        ID = id;
        Value = null;
    }

    /// <summary>
    /// Creates a new <see cref="IDString64"/> using the specified string and hashing it to create the ID.
    /// </summary>
    /// <param name="value">The string value of the IDString.</param>
    public IDString64(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ID = Murmur.Hash64(value);
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="IDString64"/> using the specified hash and original string value.
    /// </summary>
    /// <remarks>The string value is <b>not</b> checked to ensure it matches the passed <paramref name="id"/> hash.</remarks>
    /// <param name="id">The hash of the string.</param>
    /// <param name="value">The string value.</param>
    public IDString64(ulong id, string? value)
    {
#if VALIDATE_HASHES
        Debug.Assert(value is null || id == Murmur.Hash64(value), $"The specified ID {id:x16} does not match the 64-bit hash of the string value '{value}'.");
#endif
        ID = id;
        Value = value;
    }

    public override bool Equals(object? obj)
    {
        return obj is IDString64 idString && Equals(idString);
    }

    public bool Equals(IDString64 other)
    {
        return ID == other.ID;
    }

    public int CompareTo(IDString64 other)
    {
        return ID.CompareTo(other.ID);
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }

    /// <summary>
    ///     Gets the string representation of this IDString.
    /// </summary>
    /// <returns>
    ///     If <see cref="Value"/> is not <see langword="null"/>, returns <see cref="Value"/>;
    ///     otherwise, returns <see cref="ID"/> as a hexadecimal string in the format <c>0123456789abcdef</c>.
    /// </returns>
    public override string ToString()
    {
        return Value ?? ID.ToString("x16");
    }

    /// <summary>
    /// Returns the ID of this <see cref="IDString64"/> as a string in the format <c>#ID[0123456789abcdef]</c>.
    /// </summary>
    /// <remarks>This is the way the Stingray engine formats its identifier strings.</remarks>
    /// <returns>The formatted ID of this <see cref="IDString64"/>.</returns>
    public string ToIdentifier()
    {
        return $"#ID[{ID:x16}]";
    }

    public static bool operator ==(IDString64 left, IDString64 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(IDString64 left, IDString64 right)
    {
        return !(left == right);
    }

    public static bool operator <(IDString64 left, IDString64 right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(IDString64 left, IDString64 right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(IDString64 left, IDString64 right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(IDString64 left, IDString64 right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static implicit operator IDString64(string value) => new(value);
}