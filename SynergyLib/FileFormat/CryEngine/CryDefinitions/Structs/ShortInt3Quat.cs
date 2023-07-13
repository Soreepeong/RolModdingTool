using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

[StructLayout(LayoutKind.Sequential, Size = 6)]
public struct ShortInt3Quat {
    public short X;
    public short Y;
    public short Z;

    public static explicit operator ShortInt3Quat(Quaternion q) {
        if (q.W < 0) {
            q.X *= -1;
            q.Y *= -1;
            q.Z *= -1;
        }

        return new() {
            X = (short) Math.Floor(q.X * short.MaxValue + 0.5f),
            Y = (short) Math.Floor(q.Y * short.MaxValue + 0.5f),
            Z = (short) Math.Floor(q.Z * short.MaxValue + 0.5f),
        };
    }

    public static implicit operator Quaternion(ShortInt3Quat value) {
        Quaternion q = new() {
            X = 1f * value.X / short.MaxValue,
            Y = 1f * value.Y / short.MaxValue,
            Z = 1f * value.Z / short.MaxValue,
        };

        q.W = (float) Math.Sqrt(1.0f - (q.X * q.X + q.Y * q.Y + q.Z * q.Z));
        return q;
    }
}
