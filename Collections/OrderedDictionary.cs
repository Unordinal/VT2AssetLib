using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace VT2AssetLib.Collections;

/// <summary>
/// Represents a dictionary where the keys are kept in insert order.
/// </summary>
/// <remarks>Internally just wraps a List and Dictionary, where the List is of key value pairs that are used for lookups.</remarks>
/// <inheritdoc cref="IDictionary{TKey, TValue}"/>
internal sealed class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IList<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyList<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    public ICollection<TKey> Keys => CountedEnumerable.Create(_indexMap.Select(kvp => kvp.Key), Count);

    public ICollection<TValue> Values => CountedEnumerable.Create(_indexMap.Select(kvp => kvp.Value), Count);

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _indexMap.Select(kvp => kvp.Key);

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _indexMap.Select(kvp => kvp.Value);

    public int Count => _dictionary.Count;

    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
    {
        get => ElementAt(index);
        set
        {
            var kvp = ElementAt(index);
            var asColl = this as ICollection<KeyValuePair<TKey, TValue>>;
            asColl.Remove(kvp);
            asColl.Add(value);
        }
    }

    KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index] => ElementAt(index);

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    private readonly List<KeyValuePair<TKey, TValue>> _indexMap;
    private readonly Dictionary<TKey, TValue> _dictionary;

    public OrderedDictionary()
    {
        _indexMap = new();
        _dictionary = new();
    }

    public OrderedDictionary(int capacity, IEqualityComparer<TKey>? comparer = null)
    {
        _indexMap = new(capacity);
        _dictionary = new(capacity, comparer);
    }

    public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer = null)
    {
        _indexMap = new(collection);
        _dictionary = new(collection, comparer);
    }

    public void Add(TKey key, TValue value)
    {
        _dictionary.Add(key, value);
        _indexMap.Add(KeyValuePair.Create(key, value));
    }

    public void Insert(int index, KeyValuePair<TKey, TValue> item)
    {
        _indexMap.Insert(index, item);
        _dictionary.Add(item.Key, item.Value);
    }

    public bool Remove(TKey key)
    {
        return Remove(key, out _);
    }

    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!_dictionary.TryGetValue(key, out value))
            return false;

        return _dictionary.Remove(key) && _indexMap.Remove(KeyValuePair.Create(key, value));
    }

    public void RemoveAt(int index)
    {
        var kvp = ElementAt(index);
        _dictionary.Remove(kvp.Key);
        _indexMap.RemoveAt(index);
    }

    public bool ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    public bool ContainsValue(TValue value)
    {
        return _dictionary.ContainsValue(value);
    }

    public int IndexOf(KeyValuePair<TKey, TValue> value)
    {
        return _indexMap.IndexOf(value);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public void Clear()
    {
        _indexMap.Clear();
        _dictionary.Clear();
    }

    public KeyValuePair<TKey, TValue> ElementAt(int index)
    {
        return _indexMap[index];
    }

    public KeyValuePair<TKey, TValue> ElementAt(Index index)
    {
        return _indexMap[index];
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _indexMap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return _dictionary.ContainsKey(item.Key);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        _indexMap.CopyTo(array, arrayIndex);
    }
}