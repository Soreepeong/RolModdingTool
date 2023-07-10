using System.Numerics;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct CompiledPhysicalProxy {
    public uint ChunkId;
    public Vector3[] Vertices;
    public ushort[] Indices;
    public byte[] Materials;
}