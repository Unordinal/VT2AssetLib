namespace System.Collections.Generic;

public static class EnumerableExtensions
{
    public static uint Sum<T>(this IEnumerable<T> source!!, Func<T, uint> selector!!)
    {
        uint result = 0;
        foreach (var value in source)
            result += selector(value);

        return result;
    }

    public static ulong Sum<T>(this IEnumerable<T> source!!, Func<T, ulong> selector!!)
    {
        ulong result = 0;
        foreach (var value in source)
            result += selector(value);

        return result;
    }

    public static IEnumerable<T> OrderBySequence<T>(this IEnumerable<T> source, IEnumerable<T> order)
    {
        return source.OrderBySequence(order, EqualityComparer<T>.Default);
    }

    public static IEnumerable<T> OrderBySequence<T>(this IEnumerable<T> source, IEnumerable<T> order, IEqualityComparer<T>? comparer)
    {
        ArgumentNullException.ThrowIfNull(order);
        comparer ??= EqualityComparer<T>.Default;

        var lookup = source.ToHashSet(comparer);
        foreach (var item in order)
            if (lookup.Contains(item))
                yield return item;
    }

    // https://stackoverflow.com/a/15275682/
    public static IEnumerable<T> OrderBySequence<T, TId>(this IEnumerable<T> source, IEnumerable<TId> order, Func<T, TId> idSelector)
    {
        ArgumentNullException.ThrowIfNull(order);

        var lookup = source.ToLookup(idSelector, t => t);
        foreach (var id in order)
        {
            foreach (var item in lookup[id])
                yield return item;
        }
    }

    public static bool CountIsExactly<T>(this IEnumerable<T> source, int count)
    {
        if (source is ICollection<T> collT)
            return collT.Count == count;

        if (source is ICollection coll)
            return coll.Count == count;


        using var enumerator = source.GetEnumerator();
        for (int i = 0; i < count; i++)
        {
            if (!enumerator.MoveNext())
                return false;
        }

        return !enumerator.MoveNext();
    }
}