using System;
using System.Numerics;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public struct CompiledPhysicalProxy : ICryReadWrite {
    public uint ChunkId;
    public Vector3[] Vertices = Array.Empty<Vector3>();
    public ushort[] Indices = Array.Empty<ushort>();
    public byte[] Materials = Array.Empty<byte>();

    public CompiledPhysicalProxy() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        ChunkId = reader.ReadUInt32();
        Vertices = new Vector3[reader.ReadInt32()];
        Indices = new ushort[reader.ReadInt32()];
        Materials = new byte[reader.ReadInt32()];
        for (var j = 0; j < Vertices.Length; j++)
            Vertices[j] = reader.ReadVector3();
        for (var j = 0; j < Indices.Length; j++)
            Indices[j] = reader.ReadUInt16();
        for (var j = 0; j < Materials.Length; j++)
            Materials[j] = reader.ReadByte();
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(ChunkId);
            writer.Write(Vertices.Length);
            writer.Write(Indices.Length);
            writer.Write(Materials.Length);
            foreach (var x in Vertices)
                writer.Write(x);
            foreach (var x in Indices)
                writer.Write(x);
            writer.Write(Materials);
        }
    }

    public int WrittenSize => 16 + 12 * Vertices.Length + 2 * Indices.Length + Materials.Length;
}
