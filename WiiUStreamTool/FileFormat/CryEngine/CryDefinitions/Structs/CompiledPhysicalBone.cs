using System;
using System.Text;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;

public struct CompiledPhysicalBone : ICryReadWrite {
    public int BoneId;
    public int ParentId;
    public int ChildCount;
    public uint ControllerId;

    public string Properties = string.Empty;
    public PhysicsGeometry PhysicsGeometry;

    public CompiledPhysicalBone() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize == 152) {
            reader.ReadInto(out BoneId);
            reader.ReadInto(out ParentId);
            reader.ReadInto(out ChildCount);
            reader.ReadInto(out ControllerId);
            Properties = reader.ReadFString(32, Encoding.UTF8);
            PhysicsGeometry.ReadFrom(reader, 104);
        } else
            throw new NotSupportedException();
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{nameof(CompiledBone)} {ControllerId:X08}";
}
