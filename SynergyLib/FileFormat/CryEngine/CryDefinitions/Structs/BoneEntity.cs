using System;
using System.Text;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public struct BoneEntity : ICryReadWrite {
    public int BoneId;
    public int ParentId;
    public int ChildCount;
    public uint ControllerId;

    public string Properties = string.Empty;
    public CompiledBonePhysics Physics;

    public BoneEntity() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize == 152) {
            reader.ReadInto(out BoneId);
            reader.ReadInto(out ParentId);
            reader.ReadInto(out ChildCount);
            reader.ReadInto(out ControllerId);
            Properties = reader.ReadFString(32, Encoding.UTF8);
            Physics.ReadFrom(reader, 104);
        } else
            throw new NotSupportedException();
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(BoneId);
            writer.Write(ParentId);
            writer.Write(ChildCount);
            writer.Write(ControllerId);
            writer.WriteFString(Properties, 32, Encoding.UTF8);
            Physics.WriteTo(writer, useBigEndian);
        }
    }

    public int WrittenSize => 152;

    public override string ToString() => $"{nameof(CompiledBone)} {ControllerId:X08}";
}
