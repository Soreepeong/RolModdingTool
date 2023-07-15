using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public struct CompiledIntFace : IList<ushort>, IEquatable<CompiledIntFace>, IComparable<CompiledIntFace>,
    ICryReadWrite {
    public ushort Vertex0;
    public ushort Vertex1;
    public ushort Vertex2;

    public CompiledIntFace() { }

    public CompiledIntFace(ushort v0, ushort v1, ushort v2) {
        Vertex0 = v0;
        Vertex1 = v1;
        Vertex2 = v2;
    }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 6)
            throw new IOException();
        reader.ReadInto(out Vertex0);
        reader.ReadInto(out Vertex1);
        reader.ReadInto(out Vertex2);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(Vertex0);
            writer.Write(Vertex1);
            writer.Write(Vertex2);
        }
    }

    public int WrittenSize => 6;

    public IEnumerator<ushort> GetEnumerator() {
        yield return Vertex0;
        yield return Vertex1;
        yield return Vertex2;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(ushort item) => throw new NotSupportedException();

    public void Clear() => throw new NotSupportedException();

    public bool Contains(ushort item) => Vertex0 == item || Vertex1 == item || Vertex2 == item;

    public void CopyTo(ushort[] array, int arrayIndex) {
        if (arrayIndex + 3 > array.Length)
            throw new ArgumentOutOfRangeException(nameof(array), array, null);
        array[arrayIndex + 0] = Vertex0;
        array[arrayIndex + 1] = Vertex1;
        array[arrayIndex + 2] = Vertex2;
    }

    public bool Remove(ushort item) => throw new NotSupportedException();

    public int Count => 3;

    public bool IsReadOnly => false;

    public int IndexOf(ushort item) {
        if (Vertex0 == item)
            return 0;
        if (Vertex1 == item)
            return 1;
        if (Vertex2 == item)
            return 2;
        return -1;
    }

    public void Insert(int index, ushort item) => throw new NotSupportedException();

    public void RemoveAt(int index) => throw new NotSupportedException();

    public ushort this[int index] {
        get => index switch {
            0 => Vertex0,
            1 => Vertex1,
            2 => Vertex2,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
        };
        set {
            switch (index) {
                case 0:
                    Vertex0 = value;
                    break;
                case 1:
                    Vertex1 = value;
                    break;
                case 2:
                    Vertex2 = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }
        }
    }

    public int CompareTo(CompiledIntFace other) {
        var vertex0Comparison = Vertex0.CompareTo(other.Vertex0);
        if (vertex0Comparison != 0) return vertex0Comparison;
        var vertex1Comparison = Vertex1.CompareTo(other.Vertex1);
        if (vertex1Comparison != 0) return vertex1Comparison;
        return Vertex2.CompareTo(other.Vertex2);
    }

    public bool Equals(CompiledIntFace other) =>
        Vertex0 == other.Vertex0 && Vertex1 == other.Vertex1 && Vertex2 == other.Vertex2;

    public override bool Equals(object? obj) => obj is CompiledIntFace other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Vertex0, Vertex1, Vertex2);

    public static bool operator ==(CompiledIntFace a, CompiledIntFace b) =>
        a.Vertex0 == b.Vertex0 && a.Vertex1 == b.Vertex1 && a.Vertex2 == b.Vertex2;

    public static bool operator !=(CompiledIntFace a, CompiledIntFace b) =>
        a.Vertex0 == b.Vertex0 && a.Vertex1 == b.Vertex1 && a.Vertex2 == b.Vertex2;
}
