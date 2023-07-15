using System;
using System.Numerics;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public struct CompiledBonePhysics : ICryReadWrite, IEquatable<CompiledBonePhysics> {
    public uint PhysicsGeom = uint.MaxValue;
    public uint Flags = uint.MaxValue;
    public Vector3 Min = new(-1e10f, -1e10f, -1e10f);
    public Vector3 Max = new(+1e10f, +1e10f, +1e10f);
    public Vector3 SpringAngle = Vector3.Zero;
    public Vector3 SpringTension = Vector3.One;
    public Vector3 Damping = Vector3.One;
    public Matrix3x3 Framemtx = new(100, 0, 0, 0, 0, 0, 0, 0, 0);

    public CompiledBonePhysics() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize == 104) {
            PhysicsGeom = reader.ReadUInt32();
            Flags = reader.ReadUInt32();
            Min = reader.ReadVector3();
            Max = reader.ReadVector3();
            SpringAngle = reader.ReadVector3();
            SpringTension = reader.ReadVector3();
            Damping = reader.ReadVector3();
            Framemtx = reader.ReadMatrix3x3();
        } else
            throw new NotSupportedException();
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(PhysicsGeom);
            writer.Write(Flags);
            writer.Write(Min);
            writer.Write(Max);
            writer.Write(SpringAngle);
            writer.Write(SpringTension);
            writer.Write(Damping);
            writer.Write(Framemtx);
        }
    }

    public int WrittenSize => 104;

    public bool IsDefault => this == new CompiledBonePhysics();

    public bool IsEmpty =>
        PhysicsGeom == 0
        && Flags == 0
        && Min.Equals(Vector3.Zero)
        && Max.Equals(Vector3.Zero)
        && SpringAngle.Equals(Vector3.Zero)
        && SpringTension.Equals(Vector3.Zero)
        && Damping.Equals(Vector3.Zero)
        && Framemtx.Equals(Matrix3x3.Zero);

    public bool Equals(CompiledBonePhysics other) =>
        PhysicsGeom == other.PhysicsGeom
        && Flags == other.Flags
        && Min.Equals(other.Min)
        && Max.Equals(other.Max)
        && SpringAngle.Equals(other.SpringAngle)
        && SpringTension.Equals(other.SpringTension)
        && Damping.Equals(other.Damping)
        && Framemtx.Equals(other.Framemtx);

    public override bool Equals(object? obj) => obj is CompiledBonePhysics other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(
        PhysicsGeom,
        Flags,
        Min,
        Max,
        SpringAngle,
        SpringTension,
        Damping,
        Framemtx);

    public static bool operator ==(CompiledBonePhysics a, CompiledBonePhysics b) => a.Equals(b);
    public static bool operator !=(CompiledBonePhysics a, CompiledBonePhysics b) => !a.Equals(b);
}
