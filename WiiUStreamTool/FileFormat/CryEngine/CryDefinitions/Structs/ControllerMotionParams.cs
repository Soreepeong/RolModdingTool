using System.IO;
using System.Numerics;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Enums;
using WiiUStreamTool.Util.BinaryRW;
using WiiUStreamTool.Util.MathExtras;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;

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
        Compression = 0xFFFFFFFFu;
        TicksPerFrame = 0;
        SecsPerTick = 0;
        Start = 0;
        End = 0;
        MoveSpeed = -1;
        TurnSpeed = -1;
        AssetTurn = -1;
        Distance = -1;
        Slope = -1;
        StartLocationQ = Quaternion.Identity;
        StartLocationV = Vector3.One;
        EndLocationQ = Quaternion.Identity;
        EndLocationV = Vector3.One;
        LHeelStart = -1;
        LHeelEnd = -1;
        LToe0Start = -1;
        LToe0End = -1;
        RHeelStart = -1;
        RHeelEnd = -1;
        RToe0Start = -1;
        RToe0End = -1;
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