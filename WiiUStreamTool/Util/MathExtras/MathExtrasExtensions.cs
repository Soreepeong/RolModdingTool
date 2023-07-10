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

    public static Matrix4x4 ReadMatrix4x4(this BinaryReader r, FloatSize inputType = FloatSize.Single) =>
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
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble()),
            _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null),
        };
    
    public static void Write(this BinaryWriter r, in Vector3 value, FloatSize inputType = FloatSize.Single) {
        switch (inputType) {
            case FloatSize.Half:
                r.Write((Half) value.X);
                r.Write((Half) value.Y);
                r.Write((Half) value.Z);
                break;
            case FloatSize.Single:
                r.Write(value.X);
                r.Write(value.Y);
                r.Write(value.Z);
                break;
            case FloatSize.Double:
                r.Write((double) value.X);
                r.Write((double) value.Y);
                r.Write((double) value.Z);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null);
        }
    }

    public static void Write(this BinaryWriter r, in Quaternion value, FloatSize inputType = FloatSize.Single){
        switch (inputType) {
            case FloatSize.Half:
                r.Write((Half) value.X);
                r.Write((Half) value.Y);
                r.Write((Half) value.Z);
                r.Write((Half) value.W);
                break;
            case FloatSize.Single:
                r.Write(value.X);
                r.Write(value.Y);
                r.Write(value.Z);
                r.Write(value.W);
                break;
            case FloatSize.Double:
                r.Write((double) value.X);
                r.Write((double) value.Y);
                r.Write((double) value.Z);
                r.Write((double) value.W);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null);
        }
    }

    public static void Write(this BinaryWriter r, in AaBb value, FloatSize inputType = FloatSize.Single) {
        r.Write(value.Min, inputType);
        r.Write(value.Max, inputType);
    }

    public static void Write(this BinaryWriter r, in Matrix3x3 value, FloatSize inputType = FloatSize.Single) {
        r.Write(new Vector3(value.M11, value.M12, value.M13), inputType);
        r.Write(new Vector3(value.M21, value.M22, value.M23), inputType);
        r.Write(new Vector3(value.M31, value.M32, value.M33), inputType);
    }

    public static void Write(this BinaryWriter r, in Matrix3x4 value, FloatSize inputType = FloatSize.Single) {
        r.Write(new Quaternion(value.M11, value.M12, value.M13, value.M14), inputType);
        r.Write(new Quaternion(value.M21, value.M22, value.M23, value.M24), inputType);
        r.Write(new Quaternion(value.M31, value.M32, value.M33, value.M34), inputType);
    }

    public static void Write(this BinaryWriter r, in Matrix4x4 value, FloatSize inputType = FloatSize.Single) {
        r.Write(new Quaternion(value.M11, value.M12, value.M13, value.M14), inputType);
        r.Write(new Quaternion(value.M21, value.M22, value.M23, value.M24), inputType);
        r.Write(new Quaternion(value.M31, value.M32, value.M33, value.M34), inputType);
        r.Write(new Quaternion(value.M41, value.M42, value.M43, value.M44), inputType);
    }
}
