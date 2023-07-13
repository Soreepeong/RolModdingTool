using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

[StructLayout(LayoutKind.Sequential, Size = 6)]
public struct PolarQuat {
    private const int MaxPolar16B = 10430;
    private const float MaxPolar16Bf = 10430.0f;

    public short Yaw;
    public short Pitch;
    public short W;

    public PolarQuat() { }

    public static explicit operator PolarQuat(Quaternion q) {
        if (q.W < 0.0f)
            q = -q;

        float fYaw, fPitch;
        var s = MathF.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z);
        if (s > 0.0f) {
            fPitch = MathF.Acos(q.Z / s);
            var xyNorm = MathF.Sqrt(q.X * q.X + q.Y * q.Y);

            if (xyNorm <= 0.0f)
                fYaw = 0;
            else if (q.X >= 0.0f)
                fYaw = MathF.Asin(q.Y / xyNorm);
            else
                fYaw = MathF.PI - MathF.Asin(q.Y / xyNorm);
        } else
            fYaw = fPitch = 0;

        return new() {
            Yaw = (short) MathF.Floor(fYaw * MaxPolar16B + 0.5f),
            Pitch = (short) MathF.Floor(fPitch * MaxPolar16B + 0.5f),
            W = (short) MathF.Floor(q.W * short.MaxValue + 0.5f),
        };
    }

    public static implicit operator Quaternion(PolarQuat value) {
        var fW = (float) value.W / short.MaxValue;

        var fPitch = value.Pitch / MaxPolar16Bf;
        var fYaw = value.Yaw / MaxPolar16Bf;

        var (sinTheta, cosTheta) = MathF.SinCos(fPitch);
        var (sinRho, cosRho) = MathF.SinCos(fYaw);
        var fY = sinTheta * sinRho;
        var fX = sinTheta * cosRho;
        var fZ = cosTheta;

        var renorm = MathF.Sqrt(1 - fW * fW);
        fX *= renorm;
        fY *= renorm;
        fZ *= renorm;

        return new(fX, fY, fZ, fW);
    }
}
