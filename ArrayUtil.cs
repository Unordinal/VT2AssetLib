namespace VT2AssetLib;

public static class ArrayUtil
{
    /// <inheritdoc cref="Create{T}(uint)"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static T[] Create<T>(int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return Create<T>((uint)length);
    }

    /// <summary>
    ///     Creates and returns an array of the specified length.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="length">The length of the array to create.</param>
    /// <returns>
    ///     A new array with a length equal to <paramref name="length"/>,
    ///     or <see cref="Array.Empty{T}"/> if <paramref name="length"/> is zero.
    /// </returns>
    public static T[] Create<T>(uint length)
    {
        if (length == 0)
            return Array.Empty<T>();

        return new T[length];
    }

    /// <inheritdoc cref="CreateAndPopulate{T}(uint)"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static T[] CreateAndPopulate<T>(int length)
        where T : new()
    {
        return CreateAndPopulate(length, () => new T());
    }

    /// <summary>
    ///     Creates and returns an array of the specified length, initializing each element by
    ///     using the public parameterless constructor of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="length">The length of the array to create.</param>
    /// <returns>
    ///     A new array with each value initialized using the default parameterless constructor of type <typeparamref name="T"/>,
    ///     or <see cref="Array.Empty{T}"/> if <paramref name="length"/> is zero.
    /// </returns>
    public static T[] CreateAndPopulate<T>(uint length)
        where T : new()
    {
        return CreateAndPopulate(length, () => new T());
    }

    /// <inheritdoc cref="CreateAndPopulate{T}(uint, T)"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static T[] CreateAndPopulate<T>(int length, T value)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return CreateAndPopulate((uint)length, value);
    }

    /// <summary>
    ///     Creates and returns an array of the specified length, setting each value in the array
    ///     to <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="length">The length of the array to create.</param>
    /// <param name="value">The value to initialize each index to.</param>
    /// <returns>
    ///     A new array with each value equal to <paramref name="value"/>,
    ///     or <see cref="Array.Empty{T}"/> if <paramref name="length"/> is zero.
    /// </returns>
    public static T[] CreateAndPopulate<T>(uint length, T value)
    {
        if (length == 0)
            return Array.Empty<T>();

        T[] result = new T[length];
        Array.Fill(result, value);

        return result;
    }

    /// <inheritdoc cref="CreateAndPopulate{T}(uint, Func{T})"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static T[] CreateAndPopulate<T>(int length, Func<T> valueConstructor)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return CreateAndPopulate((uint)length, valueConstructor);
    }

    /// <summary>
    ///     Creates and returns an array of the specified length, intializing every index in the array
    ///     to the value returned by <paramref name="valueConstructor"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="length">The length of the array to create.</param>
    /// <param name="valueConstructor">A method to initialize each index of the array with.</param>
    /// <returns>
    ///     A new array with each value initialized using <paramref name="valueConstructor"/>,
    ///     or <see cref="Array.Empty{T}"/> if <paramref name="length"/> is zero.
    /// </returns>
    public static T[] CreateAndPopulate<T>(uint length, Func<T> valueConstructor)
    {
        ArgumentNullException.ThrowIfNull(valueConstructor);
        if (length == 0)
            return Array.Empty<T>();

        T[] result = new T[length];
        for (int i = 0; i < length; i++)
            result[i] = valueConstructor();

        return result;
    }
}