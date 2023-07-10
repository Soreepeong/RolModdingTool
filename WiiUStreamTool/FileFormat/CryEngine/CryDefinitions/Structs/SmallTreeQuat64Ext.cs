using System;
using System.Linq;
using System.Numerics;
using WiiUStreamTool.Util.MathExtras;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;

public struct SmallTreeQuat64Ext {
    private const float Max20Bit = 741454f;
    private const float Range20Bit = 0.707106781186f;
    private const float Max21Bit = 1482909.0f;
    private const float Range21Bit = 0.707106781186f;

    // Keep these separate; order of M1 and M2 is not dependent on endianness.
    public uint M1;
    public uint M2;

    public ulong Value {
        get => ((ulong) M2 << 32) | M1;
        set {
            M1 = unchecked((uint) value);
            M2 = (uint) (value >> 32);
        }
    }

    public static explicit operator SmallTreeQuat64Ext(Quaternion q) {
        var value = 0ul;

        var absoluteComponents = Enumerable.Range(0, 4).Select(x => Math.Abs(q.GetComponent(x))).ToList();
        var maxComponentIndex = absoluteComponents.IndexOf(absoluteComponents.Max());

        if (q.GetComponent(maxComponentIndex) < 0.0f)
            q = -q;

        var shift = 0;
        for (int i = 0, targetComponentIndex = 0; i < 4; ++i) {
            if (i == maxComponentIndex) continue;

            if (targetComponentIndex++ < 2) {
                value |= (ulong) Math.Floor((q.GetComponent(i) + Range21Bit) * Max21Bit + 0.5f) << shift;
                shift += 20;
            } else {
                value |= (ulong) Math.Floor((q.GetComponent(i) + Range20Bit) * Max20Bit + 0.5f) << shift;
                shift += 20;
            }
        }

        value |= (ulong) maxComponentIndex << 62;
        return new() {Value = value};
    }

    public static implicit operator Quaternion(SmallTreeQuat64Ext value) {
        var maxComponentIndex = (int) (value.Value >> 62);
        var shift = 0;
        var comp = new float[4];

        var sqrsumm = 0.0f;
        for (int i = 0, targetComponentIndex = 0; i < 4; ++i) {
            if (i == maxComponentIndex) continue;
            if (targetComponentIndex++ < 2) {
                var packed = (value.Value >> shift) & 0x1FFFFF;
                comp[i] = packed / Max21Bit - Range21Bit;
                sqrsumm += comp[i] * comp[i];
                shift += 21;
            } else {
                var packed = (value.Value >> shift) & 0xFFFFF;
                comp[i] = packed / Max20Bit - Range20Bit;
                sqrsumm += comp[i] * comp[i];
                shift += 20;
            }
        }

        comp[maxComponentIndex] = (float) Math.Sqrt(1.0f - sqrsumm);
        return new(comp[0], comp[1], comp[2], comp[3]);
    }
}
