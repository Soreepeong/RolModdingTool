using System;
using System.Collections.Generic;
using System.Numerics;

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

    public static Vector3 ToVector3(this IEnumerable<float> value) {
        using var a = value.GetEnumerator();
        var res = new Vector3 {
            X = a.MoveNext() ? a.Current : throw new ArgumentOutOfRangeException(nameof(value), value, null),
            Y = a.MoveNext() ? a.Current : throw new ArgumentOutOfRangeException(nameof(value), value, null),
            Z = a.MoveNext() ? a.Current : throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
        return res;
    }
}
