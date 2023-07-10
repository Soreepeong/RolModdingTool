using System;
using System.IO;
using System.Numerics;

namespace WiiUStreamTool.Util.MathExtras;

public static class MathExtrasExtensions {
    public static float GetComponent(this Quaternion q, int index) => index switch {
        0 => q.X,
        1 => q.Y,
        2 => q.Z,
        3 => q.W,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    public static Vector3 DropW(this Quaternion q) => new(q.X, q.Y, q.Z);

    public static Matrix3x3 ConvertToRotationMatrix(this Quaternion q) {
        // https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/index.htm
        var rotationalMatrix = new Matrix3x3();
        var sqw = q.W * q.W;
        var sqx = q.X * q.X;
        var sqy = q.Y * q.Y;
        var sqz = q.Z * q.Z;

        // invs (inverse square length) is only required if quaternion is not already normalised
        var invs = 1 / (sqx + sqy + sqz + sqw);
        rotationalMatrix.M11 = (sqx - sqy - sqz + sqw) * invs; // since sqw + sqx + sqy + sqz =1/invs*invs
        rotationalMatrix.M22 = (-sqx + sqy - sqz + sqw) * invs;
        rotationalMatrix.M33 = (-sqx - sqy + sqz + sqw) * invs;

        var tmp1 = q.X * q.Y;
        var tmp2 = q.Z * q.W;
        rotationalMatrix.M21 = 2.0f * (tmp1 + tmp2) * invs;
        rotationalMatrix.M12 = 2.0f * (tmp1 - tmp2) * invs;

        tmp1 = q.X * q.Z;
        tmp2 = q.Y * q.W;
        rotationalMatrix.M31 = 2.0f * (tmp1 - tmp2) * invs;
        rotationalMatrix.M13 = 2.0f * (tmp1 + tmp2) * invs;
        tmp1 = q.Y * q.Z;
        tmp2 = q.X * q.W;
        rotationalMatrix.M32 = 2.0f * (tmp1 + tmp2) * invs;
        rotationalMatrix.M23 = 2.0f * (tmp1 - tmp2) * invs;

        return rotationalMatrix;
    }

    public static Vector3 ReadVector3(this BinaryReader r, FloatSize inputType = FloatSize.Single) =>
        inputType switch {
            FloatSize.Half => new((float) r.ReadHalf(), (float) r.ReadHalf(), (float) r.ReadHalf()),
            FloatSize.Single => new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
            FloatSize.Double => new((float) r.ReadDouble(), (float) r.ReadDouble(), (float) r.ReadDouble()),
            _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null),
        };

    public static Quaternion ReadQuaternion(this BinaryReader r, FloatSize inputType = FloatSize.Single) =>
        inputType switch {
            FloatSize.Half => new(
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf()),
            FloatSize.Single => new(
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle()),
            FloatSize.Double => new(
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble()),
            _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null),
        };

    public static AaBb ReadAaBb(this BinaryReader r, FloatSize inputType = FloatSize.Single) =>
        inputType switch {
            FloatSize.Half => new(
                new((float) r.ReadHalf(), (float) r.ReadHalf(), (float) r.ReadHalf()),
                new((float) r.ReadHalf(), (float) r.ReadHalf(), (float) r.ReadHalf())),
            FloatSize.Single => new(
                new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
                new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle())),
            FloatSize.Double => new(
                new((float) r.ReadDouble(), (float) r.ReadDouble(), (float) r.ReadDouble()),
                new((float) r.ReadDouble(), (float) r.ReadDouble(), (float) r.ReadDouble())),
            _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null),
        };

    public static Matrix3x3 ReadMatrix3x3(this BinaryReader r, FloatSize inputType = FloatSize.Single) =>
        inputType switch {
            FloatSize.Half => new(
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf()),
            FloatSize.Single => new(
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle()),
            FloatSize.Double => new(
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble()),
            _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null),
        };

    public static Matrix3x4 ReadMatrix3x4(this BinaryReader r, FloatSize inputType = FloatSize.Single) =>
        inputType switch {
            FloatSize.Half => new(
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf()),
            FloatSize.Single => new(
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle()),
            FloatSize.Double => new(
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble()),
            _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null),
        };
}
