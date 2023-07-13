using System.Numerics;
using System.Text;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct NodeChunk : ICryChunk {
    public ChunkHeader Header { get; set; }
    public string Name = string.Empty;
    public int ObjectId;
    public int ParentId;
    public int ChildCount;
    public int MaterialId;
    public bool IsGroupHead;
    public bool IsGroupMember;
    public Matrix4x4 Transform;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public int PositionControllerId;
    public int RotationControllerId;
    public int ScaleControllerId;
    public string Properties = string.Empty;

    public NodeChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            Name = reader.ReadFString(64, Encoding.UTF8);
            reader.ReadInto(out ObjectId);
            reader.ReadInto(out ParentId);
            reader.ReadInto(out ChildCount);
            reader.ReadInto(out MaterialId);
            IsGroupHead = 0 != reader.ReadByte();
            IsGroupMember = 0 != reader.ReadByte();
            reader.EnsureZeroesOrThrow(2); // padding
            Transform = reader.ReadMatrix4x4();
            Position = reader.ReadVector3();
            Rotation = reader.ReadQuaternion();
            Scale = reader.ReadVector3();
            reader.ReadInto(out PositionControllerId);
            reader.ReadInto(out RotationControllerId);
            reader.ReadInto(out ScaleControllerId);
            Properties = reader.ReadFString(reader.ReadInt32(), Encoding.UTF8);
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.WriteFString(Name, 64, Encoding.UTF8);
            writer.Write(ObjectId);
            writer.Write(ParentId);
            writer.Write(ChildCount);
            writer.Write(MaterialId);
            writer.Write(IsGroupHead ? (byte) 1 : (byte) 0);
            writer.Write(IsGroupMember ? (byte) 1 : (byte) 0);
            writer.Write((short) 0); // 2 bytes padding
            writer.Write(Transform);
            writer.Write(Position);
            writer.Write(Rotation);
            writer.Write(Scale);
            writer.Write(PositionControllerId);
            writer.Write(RotationControllerId);
            writer.Write(ScaleControllerId);
            var propUtf8 = Encoding.UTF8.GetBytes(Properties);
            writer.Write(propUtf8.Length);
            writer.Write(propUtf8);
        }
    }

    public int WrittenSize => Header.WrittenSize + 204 + Encoding.UTF8.GetBytes(Properties).Length;

    public override string ToString() => $"{nameof(NodeChunk)}: {Header}";
}
