using System.Collections.ObjectModel;

namespace System.Collections.Generic;

public static class ListExtensions
{
    public static IReadOnlyList<T> AsReadOnly<T>(this IList<T> list)
    {
        return new ReadOnlyCollection<T>(list);
    }
}