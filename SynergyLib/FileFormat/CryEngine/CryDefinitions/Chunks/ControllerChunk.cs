using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public class ControllerChunk : ICryChunk {
    public ChunkHeader Header { get; set; } = new();
    public readonly List<ControllerKeyTime> KeyTimes = new();
    public readonly List<ControllerKeyPosition> KeyPositions = new();
    public readonly List<ControllerKeyRotation> KeyRotations = new();
    public readonly List<ControllerGroup> Animations = new();
    public int TrailingPaddingSize;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out int numKeyPos);
            reader.ReadInto(out int numKeyRot);
            reader.ReadInto(out int numKeyTime);
            reader.ReadInto(out int numAnims);

            if (0 != (numAnims & 0xFF000000)) {
                Header.IsBigEndian = reader.IsBigEndian = true;
                numKeyPos = BinaryPrimitives.ReverseEndianness(numKeyPos);
                numKeyRot = BinaryPrimitives.ReverseEndianness(numKeyRot);
                numKeyTime = BinaryPrimitives.ReverseEndianness(numKeyTime);
                numAnims = BinaryPrimitives.ReverseEndianness(numAnims);
            }

            var keyTimeLengths = new ushort[numKeyTime];
            var keyPosLengths = new ushort[numKeyPos];
            var keyRotLengths = new ushort[numKeyRot];
            var keyTimeFormats = new int[(int) KeyTimesFormat.Bitset + 1];
            var keyPosFormats = new int[(int) CompressionFormat.SmallTreeQuat64Ext + 1];
            var keyRotFormats = new int[(int) CompressionFormat.SmallTreeQuat64Ext + 1];

            reader.ReadIntoSpan(keyTimeLengths);
            reader.ReadIntoSpan(keyTimeFormats);
            reader.ReadIntoSpan(keyPosLengths);
            reader.ReadIntoSpan(keyPosFormats);
            reader.ReadIntoSpan(keyRotLengths);
            reader.ReadIntoSpan(keyRotFormats);

            var keyTimeOffsets = new int[numKeyTime + 1];
            var keyPosOffsets = new int[numKeyPos + 1];
            var keyRotOffsets = new int[numKeyRot + 1];

            reader.ReadIntoSpan(keyTimeOffsets.AsSpan(..^1));
            reader.ReadIntoSpan(keyPosOffsets.AsSpan(..^1));
            reader.ReadIntoSpan(keyRotOffsets.AsSpan(..^1));

            var trackLength = reader.ReadInt32();

            Debug.Assert(keyTimeOffsets.All(x => (x & 3) == 0));
            Debug.Assert(keyPosOffsets.All(x => (x & 3) == 0));
            Debug.Assert(keyRotOffsets.All(x => (x & 3) == 0));
            Debug.Assert((trackLength & 3) == 0);

            keyRotOffsets[^1] = trackLength;
            keyPosOffsets[^1] = keyRotOffsets.First();
            keyTimeOffsets[^1] = keyPosOffsets.First();

            var trackOffset = reader.BaseStream.Position;
            if ((trackOffset & 3) != 0)
                trackOffset = (trackOffset & ~3) + 4;

            KeyTimes.Clear();
            KeyTimes.EnsureCapacity(numKeyTime);
            for (int i = 0, j = 0; i < keyTimeLengths.Length && j < keyTimeFormats.Length; i++) {
                reader.BaseStream.Position = trackOffset + keyTimeOffsets[i];
                while (j < keyTimeFormats.Length && keyTimeFormats[j] == 0)
                    j++;
                if (j == keyTimeFormats.Length)
                    throw new InvalidDataException("sum(count per format) != count of keytimes");
                keyTimeFormats[j]--;

                var c = new ControllerKeyTime();
                c.ReadFrom(reader, (KeyTimesFormat) j, keyTimeLengths[i]);
                KeyTimes.Add(c);
            }

            if (KeyTimes.Count != numKeyTime || keyTimeFormats.Any(x => x != 0))
                throw new InvalidDataException("sum(count per format) != count of keytimes");

            KeyPositions.Clear();
            KeyPositions.EnsureCapacity(numKeyPos);
            for (int i = 0, j = 0; i < keyPosLengths.Length && j < keyPosFormats.Length; i++) {
                reader.BaseStream.Position = trackOffset + keyPosOffsets[i];
                while (j < keyPosFormats.Length && keyPosFormats[j] == 0)
                    j++;
                if (j == keyPosFormats.Length)
                    throw new InvalidDataException("sum(count per format) != count of keytimes");
                keyPosFormats[j]--;

                var c = new ControllerKeyPosition();
                c.ReadFrom(reader, (CompressionFormat) j, keyPosLengths[i]);
                KeyPositions.Add(c);
            }

            if (KeyPositions.Count != numKeyPos || keyPosFormats.Any(x => x != 0))
                throw new InvalidDataException("sum(count per format) != count of keypos");

            KeyRotations.Clear();
            KeyRotations.EnsureCapacity(numKeyRot);
            for (int i = 0, j = 0; i < keyRotLengths.Length && j < keyRotFormats.Length; i++) {
                reader.BaseStream.Position = trackOffset + keyRotOffsets[i];
                while (j < keyRotFormats.Length && keyRotFormats[j] == 0)
                    j++;
                if (j == keyRotFormats.Length)
                    throw new InvalidDataException("sum(count per format) != count of keytimes");
                keyRotFormats[j]--;

                var c = new ControllerKeyRotation();
                c.ReadFrom(reader, (CompressionFormat) j, keyRotLengths[i]);
                KeyRotations.Add(c);
            }

            if (KeyRotations.Count != numKeyRot || keyRotFormats.Any(x => x != 0))
                throw new InvalidDataException("sum(count per format) != count of keyRot");

            reader.BaseStream.Position = trackOffset + trackLength;

            Animations.Clear();
            Animations.EnsureCapacity(numAnims);
            for (var i = 0; i < numAnims; i++) {
                var a = new ControllerGroup();
                a.ReadFrom(reader, -1);
                Animations.Add(a);
            }
        }

        // seems that it allocates buffer a bit too much
        reader.EnsureZeroesOrThrow(TrailingPaddingSize = checked((int) (expectedEnd - reader.BaseStream.Position)));
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(KeyPositions.Count);
            writer.Write(KeyRotations.Count);
            writer.Write(KeyTimes.Count);
            writer.Write(Animations.Count);

            foreach (var x in KeyTimes)
                writer.Write(checked((ushort) x.LengthMarker));
            for (var i = 0; i <= (int) KeyTimesFormat.Bitset; i++)
                writer.Write(KeyTimes.Count(x => (int) x.Format == i));

            foreach (var x in KeyPositions)
                writer.Write(checked((ushort) x.Data.Length));
            for (var i = 0; i <= (int) CompressionFormat.SmallTreeQuat64Ext; i++)
                writer.Write(KeyPositions.Count(x => (int) x.Format == i));

            foreach (var x in KeyRotations)
                writer.Write(checked((ushort) x.Count));
            for (var i = 0; i <= (int) CompressionFormat.SmallTreeQuat64Ext; i++)
                writer.Write(KeyRotations.Count(x => (int) x.Format == i));

            var innerOffset = 0;
            foreach (var x in KeyTimes) {
                writer.Write(innerOffset);
                innerOffset += (x.WrittenSize + 3) / 4 * 4;
            }

            foreach (var x in KeyPositions) {
                writer.Write(innerOffset);
                innerOffset += (x.WrittenSize + 3) / 4 * 4;
            }

            foreach (var x in KeyRotations) {
                writer.Write(innerOffset);
                innerOffset += (x.WrittenSize + 3) / 4 * 4;
            }

            writer.Write(innerOffset);

            writer.WritePadding(4);
            foreach (var x in KeyTimes) {
                x.WriteTo(writer);
                writer.WritePadding(4);
            }

            foreach (var x in KeyPositions) {
                x.WriteTo(writer);
                writer.WritePadding(4);
            }

            foreach (var x in KeyRotations) {
                x.WriteTo(writer);
                writer.WritePadding(4);
            }

            foreach (var a in Animations)
                a.WriteTo(writer, useBigEndian);

            writer.FillZeroes(TrailingPaddingSize);
        }
    }

    public int WrittenSize {
        get {
            var ptr =
                Header.WrittenSize +
                // Counts
                16 +
                // Lengths
                2 * (KeyTimes.Count + KeyPositions.Count + KeyRotations.Count) +
                // Formats
                4 * ((int) KeyTimesFormat.Bitset + 1 + (int) CompressionFormat.SmallTreeQuat64Ext * 2 + 2) +
                // Offsets
                4 * (KeyTimes.Count + KeyPositions.Count + KeyRotations.Count) +
                // Track Length
                4;
            ptr = (ptr + 3) / 4 * 4;

            foreach (var x in KeyTimes)
                ptr = (ptr + x.WrittenSize + 3) / 4 * 4;
            foreach (var x in KeyPositions)
                ptr = (ptr + x.WrittenSize + 3) / 4 * 4;
            foreach (var x in KeyRotations)
                ptr = (ptr + x.WrittenSize + 3) / 4 * 4;

            ptr += Animations.Sum(x => x.WrittenSize);
            ptr += TrailingPaddingSize;
            return ptr;
        }
    }

    public override string ToString() => $"{nameof(ControllerChunk)}: {Header}";
}
