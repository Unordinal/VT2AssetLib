using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

public static class DictionaryExtensions
{
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
    {
        if (!dict.TryGetValue(key, out TValue? value))
        {
            value = new TValue();
            dict.Add(key, value);
        }

        return value;
    }

    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> constructor)
    {
        ArgumentNullException.ThrowIfNull(constructor);
        if (!dict.TryGetValue(key, out TValue? value))
        {
            value = constructor();
            dict.Add(key, value);
        }

        return value;
    }

    [return: NotNullIfNotNull("defaultValue")]
    public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue? defaultValue = default) where TKey : notnull
    {
        if (dict.TryGetValue(key, out TValue? value))
            return value;

        return defaultValue;
    }
}