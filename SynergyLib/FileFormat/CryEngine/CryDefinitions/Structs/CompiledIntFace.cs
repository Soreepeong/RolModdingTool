using System.IO;
using System.Runtime.InteropServices;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct CompiledIntFace : ICryReadWrite {
    [FieldOffset(0)] public ushort Face0;
    [FieldOffset(2)] public ushort Face1;
    [FieldOffset(4)] public ushort Face2;
    [FieldOffset(0)] public unsafe fixed ushort Faces[3];

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 6)
            throw new IOException();
        reader.ReadInto(out Face0);
        reader.ReadInto(out Face1);
        reader.ReadInto(out Face2);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(Face0);
            writer.Write(Face1);
            writer.Write(Face2);
        }
    }

    public int WrittenSize => 6;
}
