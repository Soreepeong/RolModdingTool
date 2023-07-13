using System.IO;
using System.Numerics;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

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

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(FirstIndexId);
            writer.Write(NumIndices);
            writer.Write(FirstVertId);
            writer.Write(NumVerts);
            writer.Write(MatId);
            writer.Write(Radius);
            writer.Write(Center);
        }
    }

    public int WrittenSize => 36;
}
