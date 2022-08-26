using System.Collections;

namespace VT2AssetLib.Collections;

/// <summary>
/// Wraps an <see cref="IEnumerable{T}"/> as an <see cref="ICollection{T}"/> by providing an upfront count.
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class CountedEnumerable<T> : ICollection<T>
{
    public int Count { get; }

    public bool IsReadOnly => true;

    private readonly IEnumerable<T> _collection;

    /// <inheritdoc cref="CountedEnumerable.Create{T}(IEnumerable{T}, int)"/>
    /// 
    public CountedEnumerable(IEnumerable<T> collection, int count)
    {
        ArgumentNullException.ThrowIfNull(collection);
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        _collection = collection;
        Count = count;
    }

    public void Add(T item)
    {
        throw new InvalidOperationException();
    }

    public void Clear()
    {
        throw new InvalidOperationException();
    }

    public bool Contains(T item)
    {
        return _collection.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (_collection is ICollection<T> coll)
        {
            coll.CopyTo(array, arrayIndex);
        }
        else
        {
            int i = arrayIndex;
            foreach (var item in _collection)
            {
                array[i] = item;
                i++;
            }
        }
    }

    public bool Remove(T item)
    {
        throw new InvalidOperationException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _collection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

/// <summary>
/// Provides utility methods for <see cref="CountedEnumerable{T}"/>; a wrapper for enumerables that have a predefined count.
/// </summary>
internal static class CountedEnumerable
{
    /// <summary>
    /// Creates a new <see cref="CountedEnumerable{T}"/> instance by providing an <see cref="IEnumerable{T}"/>
    /// and a count.
    /// </summary>
    /// <remarks>If a count cannot be provided without enumeration, consider using a regular read-only collection class instead.</remarks>
    /// <param name="collection">The enumerable to wrap.</param>
    /// <param name="count">The number of items in the enumerable. This is not validated.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static CountedEnumerable<T> Create<T>(IEnumerable<T> collection, int count)
    {
        return new CountedEnumerable<T>(collection, count);
    }
}