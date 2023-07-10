using System;
using System.Linq;
using System.Numerics;
using WiiUStreamTool.Util.MathExtras;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;

public struct SmallTreeQuat32 {
    private const float Max10Bit = 723.0f;
    private const float Range10Bit = 0.707106781186f;

    public uint Value;

    public static explicit operator SmallTreeQuat32(Quaternion q) {
        var value = 0u;

        var absoluteComponents = Enumerable.Range(0, 4).Select(x => Math.Abs(q.GetComponent(x))).ToList();
        var maxComponentIndex = absoluteComponents.IndexOf(absoluteComponents.Max());

        if (q.GetComponent(maxComponentIndex) < 0.0f)
            q = -q;

        var shift = 0;
        for (var i = 0; i < 4; ++i) {
            if (i == maxComponentIndex) continue;
            value |= (uint) Math.Floor((q.GetComponent(i) + Range10Bit) * Max10Bit + 0.5f) << shift;
            shift += 10;
        }

        value |= (uint) maxComponentIndex << shift;
        return new() {Value = value};
    }

    public static implicit operator Quaternion(SmallTreeQuat32 value) {
        var maxComponentIndex = (int) (value.Value >> 30);
        var shift = 0;
        var comp = new float[4];

        var sqrsumm = 0.0f;
        for (var i = 0; i < 4; ++i) {
            if (i == maxComponentIndex) continue;
            var packed = (value.Value >> shift) & 0x3FF;
            comp[i] = packed / Max10Bit - Range10Bit;
            sqrsumm += comp[i] * comp[i];
            shift += 10;
        }

        comp[maxComponentIndex] = (float) Math.Sqrt(1.0f - sqrsumm);
        return new(comp[0], comp[1], comp[2], comp[3]);
    }
}
