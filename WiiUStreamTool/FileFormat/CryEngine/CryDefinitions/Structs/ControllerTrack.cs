using System.IO;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;

public struct ControllerTrack : ICryReadWrite {
    public const int InvalidTrack = -1;
    public uint ControllerId;
    public int PosKeyTimeTrack;
    public int PosTrack;
    public int RotKeyTimeTrack;
    public int RotTrack;

    public bool HasPosTrack => PosTrack != InvalidTrack && PosKeyTimeTrack != InvalidTrack;
    public bool HasRotTrack => RotTrack != InvalidTrack && RotKeyTimeTrack != InvalidTrack;

    public ControllerTrack() {
        ControllerId = uint.MaxValue;
        PosKeyTimeTrack = PosTrack = RotKeyTimeTrack = RotTrack = InvalidTrack;
    }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 20)
            throw new IOException();

        ControllerId = reader.ReadUInt32();
        PosKeyTimeTrack = reader.ReadInt32();
        PosTrack = reader.ReadInt32();
        RotKeyTimeTrack = reader.ReadInt32();
        RotTrack = reader.ReadInt32();
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(ControllerId);
            writer.Write(PosKeyTimeTrack);
            writer.Write(PosTrack);
            writer.Write(RotKeyTimeTrack);
            writer.Write(RotTrack);
        }
    }

    public int WrittenSize => 20;
}
