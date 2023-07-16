using System.IO;
using System.Numerics;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public struct ControllerMotionParams : ICryReadWrite {
    public AssetFlags AssetFlags;
    public uint Compression;

    public int TicksPerFrame;
    public float SecsPerTick;
    public int Start;
    public int End;

    public float MoveSpeed;
    public float TurnSpeed;
    public float AssetTurn;
    public float Distance;
    public float Slope;

    public Quaternion StartLocationQ;
    public Vector3 StartLocationV;
    public Quaternion EndLocationQ;
    public Vector3 EndLocationV;

    public float LHeelStart;
    public float LHeelEnd;
    public float LToe0Start;
    public float LToe0End;
    public float RHeelStart;
    public float RHeelEnd;
    public float RToe0Start;
    public float RToe0End;

    public ControllerMotionParams() {
        AssetFlags = 0;
        Compression = 2;
        TicksPerFrame = 1;
        SecsPerTick = 1f / 30f;
        Start = 1;
        End = 1;
        MoveSpeed = 0;
        TurnSpeed = 1.14449864e-15f;
        AssetTurn = 2.84217162e-14f;
        Distance = 0;
        Slope = 0;
        StartLocationQ = Quaternion.Identity;
        StartLocationV = Vector3.Zero;
        EndLocationQ = Quaternion.Identity;
        EndLocationV = Vector3.Zero;
        LHeelStart = -10000;
        LHeelEnd = -10000;
        LToe0Start = -10000;
        LToe0End = -10000;
        RHeelStart = -10000;
        RHeelEnd = -10000;
        RToe0Start = -10000;
        RToe0End = -10000;
    }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 132)
            throw new IOException();

        AssetFlags = (AssetFlags) reader.ReadUInt32();
        Compression = reader.ReadUInt32();
        TicksPerFrame = reader.ReadInt32();
        SecsPerTick = reader.ReadSingle();
        Start = reader.ReadInt32();
        End = reader.ReadInt32();
        MoveSpeed = reader.ReadSingle();
        TurnSpeed = reader.ReadSingle();
        AssetTurn = reader.ReadSingle();
        Distance = reader.ReadSingle();
        Slope = reader.ReadSingle();
        StartLocationQ = reader.ReadQuaternion();
        StartLocationV = reader.ReadVector3();
        EndLocationQ = reader.ReadQuaternion();
        EndLocationV = reader.ReadVector3();
        LHeelStart = reader.ReadSingle();
        LHeelEnd = reader.ReadSingle();
        LToe0Start = reader.ReadSingle();
        LToe0End = reader.ReadSingle();
        RHeelStart = reader.ReadSingle();
        RHeelEnd = reader.ReadSingle();
        RToe0Start = reader.ReadSingle();
        RToe0End = reader.ReadSingle();
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.WriteEnum(AssetFlags);
            writer.Write(Compression);
            writer.Write(TicksPerFrame);
            writer.Write(SecsPerTick);
            writer.Write(Start);
            writer.Write(End);
            writer.Write(MoveSpeed);
            writer.Write(TurnSpeed);
            writer.Write(AssetTurn);
            writer.Write(Distance);
            writer.Write(Slope);
            writer.Write(StartLocationQ);
            writer.Write(StartLocationV);
            writer.Write(EndLocationQ);
            writer.Write(EndLocationV);
            writer.Write(LHeelStart);
            writer.Write(LHeelEnd);
            writer.Write(LToe0Start);
            writer.Write(LToe0End);
            writer.Write(RHeelStart);
            writer.Write(RHeelEnd);
            writer.Write(RToe0Start);
            writer.Write(RToe0End);
        }
    }

    public int WrittenSize => 132;
}
