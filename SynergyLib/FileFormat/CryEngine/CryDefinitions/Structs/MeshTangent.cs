using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct MeshTangent : IEquatable<MeshTangent> {
    [FieldOffset(0)]
    private unsafe fixed uint ValueUIntArray[4];
    
    [FieldOffset(0)]
    private unsafe fixed ulong ValueLongArray[2];

    [FieldOffset(0)]
    public Vector4<short> TangentRaw;

    [FieldOffset(8)]
    public Vector4<short> BinormalRaw;

    public MeshTangent() { }

    public MeshTangent(Vector4 tangent, Vector4 binormal) {
        Tangent = tangent;
        Binormal = binormal;
    }

    public Vector4 Tangent {
        get => new(
            1f * TangentRaw.X / short.MaxValue,
            1f * TangentRaw.Y / short.MaxValue,
            1f * TangentRaw.Z / short.MaxValue,
            1f * TangentRaw.W / short.MaxValue);
        set => TangentRaw = new(
            (short) (value.X * short.MaxValue),
            (short) (value.Y * short.MaxValue),
            (short) (value.Z * short.MaxValue),
            (short) (value.W * short.MaxValue));
    }

    public Vector4 Binormal {
        get => new(
            1f * BinormalRaw.X / short.MaxValue,
            1f * BinormalRaw.Y / short.MaxValue,
            1f * BinormalRaw.Z / short.MaxValue,
            1f * BinormalRaw.W / short.MaxValue);
        set => BinormalRaw = new(
            (short) (value.X * short.MaxValue),
            (short) (value.Y * short.MaxValue),
            (short) (value.Z * short.MaxValue),
            (short) (value.W * short.MaxValue));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool Equals(MeshTangent other) =>
        ValueLongArray[0] == other.ValueLongArray[0] && ValueLongArray[1] == other.ValueLongArray[1];

    public override bool Equals(object? obj) => obj is MeshTangent other && Equals(other);

    public override unsafe int GetHashCode() => unchecked((int) (
        ValueUIntArray[0]
        | BitOperations.RotateLeft(ValueUIntArray[1], 7)
        | BitOperations.RotateLeft(ValueUIntArray[2], 17)
        | BitOperations.RotateLeft(ValueUIntArray[3], 29)));

    public override string ToString() => $"Tan={Tangent}, Bi={Binormal}";

    public static bool operator ==(MeshTangent left, MeshTangent right) => left.Equals(right);

    public static bool operator !=(MeshTangent left, MeshTangent right) => !left.Equals(right);

    public static MeshTangent FromNormalAndTangent(Vector3 normal, Vector4 tangent) =>
        new(tangent, new(Vector3.Cross(tangent.DropW(), normal), -1));
    
    public static MeshTangent FromNormalAndBinormal(Vector3 normal, Vector4 binormal) =>
        new(new(Vector3.Cross(normal, binormal.DropW()), -1), binormal);
}
