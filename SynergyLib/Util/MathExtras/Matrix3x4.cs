using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SynergyLib.Util.MathExtras;

/// <summary>
/// WorldToBone and BoneToWorld objects in Cryengine files.  Inspiration/code based from
/// https://referencesource.microsoft.com/#System.Numerics/System/Numerics/Matrix4x4.cs,48ce53b7e55d0436
///
/// Row-Major. Matrix4x4 is Column-Major.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 48)]
// ReSharper disable once InconsistentNaming
public struct Matrix3x4 : IEquatable<Matrix3x4> {
    [FieldOffset(0)]
    private unsafe fixed uint UIntCells[16];

    #region Float Fields

    [FieldOffset(0)]
    public unsafe fixed float FixedCells[16];

    [FieldOffset(0)]
    public float M11;

    [FieldOffset(4)]
    public float M12;

    [FieldOffset(8)]
    public float M13;

    [FieldOffset(12)]
    public float M14;

    [FieldOffset(16)]
    public float M21;

    [FieldOffset(20)]
    public float M22;

    [FieldOffset(24)]
    public float M23;

    [FieldOffset(28)]
    public float M24;

    [FieldOffset(32)]
    public float M31;

    [FieldOffset(36)]
    public float M32;

    [FieldOffset(40)]
    public float M33;

    [FieldOffset(44)]
    public float M34;

    #endregion

    #region Vector4 Fields

    [FieldOffset(0)]
    public Vector4 Column1;

    [FieldOffset(16)]
    public Vector4 Column2;

    [FieldOffset(32)]
    public Vector4 Column3;

    #endregion

    #region Constructors

    public Matrix3x4() { }

    public Matrix3x4(
        float m11,
        float m12,
        float m13,
        float m14,
        float m21,
        float m22,
        float m23,
        float m24,
        float m31,
        float m32,
        float m33,
        float m34) {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M14 = m14;

        M21 = m21;
        M22 = m22;
        M23 = m23;
        M24 = m24;

        M31 = m31;
        M32 = m32;
        M33 = m33;
        M34 = m34;
    }

    public unsafe Matrix3x4(IEnumerable<float> cells) {
        const string err = "Insufficient number of items.";
        using var e = cells.GetEnumerator();
        for (var i = 0; i < 12; i++)
            FixedCells[i] = e.MoveNext() ? e.Current : throw new ArgumentException(err, nameof(cells));
    }

    public Matrix3x4(Vector4 col1, Vector4 col2, Vector4 col3) {
        Column1 = col1;
        Column2 = col2;
        Column3 = col3;
    }

    public Matrix3x4(IEnumerable<Vector4> columns) {
        const string err = "Insufficient number of items.";
        using var e = columns.GetEnumerator();
        Column1 = e.MoveNext() ? e.Current : throw new ArgumentException(err, nameof(columns));
        Column2 = e.MoveNext() ? e.Current : throw new ArgumentException(err, nameof(columns));
        Column3 = e.MoveNext() ? e.Current : throw new ArgumentException(err, nameof(columns));
    }

    #endregion

    public Vector3 Translation {
        get => new(M14, M24, M34);
        set => (M14, M24, M34) = (value.X, value.Y, value.Z);
    }

    public Matrix3x3 Rotation => new(M11, M12, M13, M21, M22, M23, M31, M32, M33);

    public Matrix4x4 Transformation => new(M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, 0, 0, 0, 1);

    public static Matrix3x4 CreateFromMatrix4x4(Matrix4x4 m) => new(
        m.M11,
        m.M12,
        m.M13,
        m.M14,
        m.M21,
        m.M22,
        m.M23,
        m.M24,
        m.M31,
        m.M32,
        m.M33,
        m.M34);

    /// <summary>
    /// Creates a rotation matrix from the given Quaternion rotation value.
    /// </summary>
    /// <param name="quaternion">The source Quaternion.</param>
    /// <returns>The rotation matrix.</returns>
    public static Matrix3x4 CreateFromQuaternion(Quaternion quaternion) {
        var rot = quaternion.ConvertToRotationMatrix();
        return new() {
            M11 = rot.M11,
            M12 = rot.M12,
            M13 = rot.M13,
            M14 = 0,
            M21 = rot.M21,
            M22 = rot.M22,
            M23 = rot.M23,
            M24 = 0,
            M31 = rot.M31,
            M32 = rot.M32,
            M33 = rot.M33,
            M34 = 0
        };
    }

    public static Matrix3x4 CreateFromParts(Quaternion quaternion, Vector3 translation) {
        Matrix3x4 result = CreateFromQuaternion(quaternion);
        result.Translation = translation;

        return result;
    }

    /// <summary>
    /// Returns a boolean indicating whether this matrix instance is equal to the other given matrix.
    /// </summary>
    /// <param name="other">The matrix to compare this instance to.</param>
    /// <returns>True if the matrices are equal; False otherwise.</returns>
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public bool Equals(Matrix3x4 other) =>
        M11 == other.M11 &&
        M22 == other.M22 &&
        M33 == other.M33 && // Check diagonal element first for early out.
        M12 == other.M12 &&
        M13 == other.M13 &&
        M14 == other.M14 &&
        M21 == other.M21 &&
        M23 == other.M23 &&
        M24 == other.M24 &&
        M31 == other.M31 &&
        M32 == other.M32 &&
        M34 == other.M34;

    public override bool Equals(object? obj) => obj is Matrix3x4 b && Equals(b);

    public override unsafe int GetHashCode() => unchecked((int) (
        BitOperations.RotateLeft(UIntCells[0], 0) ^
        BitOperations.RotateLeft(UIntCells[1], 3) ^
        BitOperations.RotateLeft(UIntCells[2], 6) ^
        BitOperations.RotateLeft(UIntCells[3], 10) ^
        BitOperations.RotateLeft(UIntCells[4], 12) ^
        BitOperations.RotateLeft(UIntCells[5], 15) ^
        BitOperations.RotateLeft(UIntCells[6], 17) ^
        BitOperations.RotateLeft(UIntCells[7], 20) ^
        BitOperations.RotateLeft(UIntCells[8], 22) ^
        BitOperations.RotateLeft(UIntCells[9], 25) ^
        BitOperations.RotateLeft(UIntCells[10], 27) ^
        BitOperations.RotateLeft(UIntCells[11], 30)));
}
