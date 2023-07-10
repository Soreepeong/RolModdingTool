using System.IO;
using System.Numerics;
using WiiUStreamTool.Util.BinaryRW;
using WiiUStreamTool.Util.MathExtras;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;

public struct CompiledMorphTarget : ICryReadWrite {
    public uint VertexId;
    public Vector3 Vertex;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 16)
            throw new IOException();
        VertexId = reader.ReadUInt32();
        Vertex = reader.ReadVector3();
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(VertexId);
            writer.Write(Vertex);
        }
    }

    public int WrittenSize => 16;
}
