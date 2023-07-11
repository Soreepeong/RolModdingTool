using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using WiiUStreamTool.Util.MathExtras;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;

[StructLayout(LayoutKind.Sequential, Size = 6)]
public struct SmallTreeQuat48 {
    private const float Max15Bit = 23170.0f;
    private const float Range15Bit = 0.707106781186f;

    public ushort M1, M2, M3;

    public static explicit operator SmallTreeQuat48(Quaternion q) {
        var value = 0ul;

        var absoluteComponents = Enumerable.Range(0, 4).Select(x => Math.Abs(q.GetComponent(x))).ToList();
        var maxComponentIndex = absoluteComponents.IndexOf(absoluteComponents.Max());

        if (q.GetComponent(maxComponentIndex) < 0.0f)
            q = -q;

        var shift = 0;
        for (var i = 0; i < 4; ++i) {
            if (i == maxComponentIndex) continue;
            value |= (ulong) Math.Floor((q.GetComponent(i) + Range15Bit) * Max15Bit + 0.5f) << shift;
            shift += 15;
        }

        value |= (ulong) maxComponentIndex << 46;
        return new() {
            M1 = (ushort) (value & 0xFFFF),
            M2 = (ushort) ((value >> 16) & 0xFFFF),
            M3 = (ushort) ((value >> 32) & 0xFFFF),
        };
    }

    public static implicit operator Quaternion(SmallTreeQuat48 value) {
        var v64 = ((ulong) value.M3 << 32) | ((ulong) value.M2 << 16) | value.M1;
        var maxComponentIndex = (int) (v64 >> 46);
        var shift = 0;
        var comp = new float[4];

        var sqrsumm = 0.0f;
        for (var i = 0; i < 4; ++i) {
            if (i == maxComponentIndex) continue;
            var packed = (v64 >> shift) & 0x7FFF;
            comp[i] = packed / Max15Bit - Range15Bit;
            sqrsumm += comp[i] * comp[i];
            shift += 15;
        }

        comp[maxComponentIndex] = (float) Math.Sqrt(1.0f - sqrsumm);
        return new(comp[0], comp[1], comp[2], comp[3]);
    }
}
