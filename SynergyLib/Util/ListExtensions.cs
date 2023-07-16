using System.Collections.Generic;

namespace SynergyLib.Util;

public static class ListExtensions {
    public static int AddAndGetIndex<T>(this IList<T> list, T value) {
        list.Add(value);
        return list.Count - 1;
    }

    public static int AddRangeAndGetIndex<T>(this List<T> list, IEnumerable<T> value) {
        var i = list.Count;
        list.AddRange(value);
        return i;
    }
}
