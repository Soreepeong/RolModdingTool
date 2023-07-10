using System;
using System.IO;
using System.Numerics;
using WiiUStreamTool.Util.BinaryRW;
using WiiUStreamTool.Util.MathExtras;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct MeshSubset : ICryReadWrite {
    public int FirstIndexId;
    public int NumIndices;
    public int FirstVertId;
    public int NumVerts;
    public int MatId; // Material sub-object Id.
    public float Radius;
    public Vector3 Center;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 36)
            throw new IOException();
        reader.ReadInto(out FirstIndexId);
        reader.ReadInto(out NumIndices);
        reader.ReadInto(out FirstVertId);
        reader.ReadInto(out NumVerts);
        reader.ReadInto(out MatId);
        reader.ReadInto(out Radius);
        Center = reader.ReadVector3();
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }
}