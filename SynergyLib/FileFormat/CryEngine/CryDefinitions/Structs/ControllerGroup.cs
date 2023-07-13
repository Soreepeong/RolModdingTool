using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public struct ControllerGroup : ICryReadWrite {
    public string Name = string.Empty;
    public ControllerMotionParams MotionParams;
    public byte[] FootPlanBits = Array.Empty<byte>();
    public readonly List<ControllerTrack> Controllers = new();

    public ControllerGroup() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var nameLen = reader.ReadUInt16();
        Name = Encoding.UTF8.GetString(reader.ReadBytes(nameLen));

        MotionParams.ReadFrom(reader, 132);

        var footPlantBitsCount = reader.ReadUInt16();
        FootPlanBits = reader.ReadBytes(footPlantBitsCount);

        var controllerCount = reader.ReadUInt16();
        Controllers.Clear();
        Controllers.EnsureCapacity(controllerCount);
        for (var j = 0u; j < controllerCount; j++) {
            var c = new ControllerTrack();
            c.ReadFrom(reader, 20);
            Controllers.Add(c);
        }
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            var nameBytes = Encoding.UTF8.GetBytes(Name);
            writer.Write(checked((ushort) nameBytes.Length));
            writer.Write(nameBytes);

            MotionParams.WriteTo(writer, useBigEndian);

            writer.Write(checked((ushort) FootPlanBits.Length));
            writer.Write(FootPlanBits);

            writer.Write(checked((ushort) Controllers.Count));
            foreach (var c in Controllers)
                c.WriteTo(writer, useBigEndian);
            ;
        }
    }

    public int WrittenSize =>
        2 + Encoding.UTF8.GetBytes(Name).Length +
        MotionParams.WrittenSize +
        2 + FootPlanBits.Length +
        2 + Controllers.Sum(x => x.WrittenSize);

    public override string ToString() => $"{nameof(ControllerGroup)}: {Name} ({Controllers.Count} controllers)";
}
