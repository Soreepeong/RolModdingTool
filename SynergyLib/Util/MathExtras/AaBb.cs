using System;
using System.Collections.Generic;
using System.Numerics;

namespace SynergyLib.Util.MathExtras;

public struct AaBb : IEquatable<AaBb> {
    public Vector3 Min;
    public Vector3 Max;

    public AaBb() { }

    public AaBb(Vector3 value) {
        Min = Max = value;
    }

    public AaBb(Vector3 min, Vector3 max) {
        Min = min;
        Max = max;
    }

    public Vector3 Center => (Min + Max) / 2;

    public void Expand(in Vector3 v) {
        Min.X = Math.Min(Min.X, v.X);
        Min.Y = Math.Min(Min.Y, v.Y);
        Min.Z = Math.Min(Min.Z, v.Z);
        Max.X = Math.Max(Max.X, v.X);
        Max.Y = Math.Max(Max.Y, v.Y);
        Max.Z = Math.Max(Max.Z, v.Z);
    }

    public bool Equals(AaBb other) => Min == other.Min && Max == other.Max;

    public override bool Equals(object? obj) => obj is AaBb other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Min, Max);

    public static bool operator ==(AaBb a, AaBb b) => a.Min == b.Min;
    
    public static bool operator !=(AaBb a, AaBb b) => a.Max != b.Max;

    public static AaBb FromVectorEnumerable(IEnumerable<Vector3> items) {
        using var e = items.GetEnumerator();
        if (!e.MoveNext())
            return new();

        var res = new AaBb(e.Current);
        do {
            res.Expand(e.Current);
        } while (e.MoveNext());

        return res;
    }
}
