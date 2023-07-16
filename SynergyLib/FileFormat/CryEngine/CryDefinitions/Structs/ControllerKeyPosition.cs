using System;
using System.Numerics;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public class ControllerKeyPosition {
    public VectorCompressionFormat Format;
    public Vector3[] Data = Array.Empty<Vector3>();

    public void ReadFrom(NativeReader b, VectorCompressionFormat format, int length) {
        var data = new Vector3[length];
        switch (format) {
            case VectorCompressionFormat.NoCompressVec3:
                for (var i = 0; i < length; i++)
                    data[i] = b.ReadVector3();
                break;
            case VectorCompressionFormat.NoCompress:
            case VectorCompressionFormat.NoCompressQuat:
            case VectorCompressionFormat.ShortInt3Quat:
            case VectorCompressionFormat.SmallTreeQuat32:
            case VectorCompressionFormat.SmallTreeQuat48:
            case VectorCompressionFormat.SmallTreeQuat64:
            case VectorCompressionFormat.PolarQuat:
            case VectorCompressionFormat.SmallTreeQuat64Ext:
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        Format = format;
        Data = data;
    }

    public void WriteTo(NativeWriter w) {
        switch (Format) {
            case VectorCompressionFormat.NoCompressVec3:
                foreach (var v in Data)
                    w.Write(v);
                break;
            case VectorCompressionFormat.NoCompress:
            case VectorCompressionFormat.NoCompressQuat:
            case VectorCompressionFormat.ShortInt3Quat:
            case VectorCompressionFormat.SmallTreeQuat32:
            case VectorCompressionFormat.SmallTreeQuat48:
            case VectorCompressionFormat.SmallTreeQuat64:
            case VectorCompressionFormat.PolarQuat:
            case VectorCompressionFormat.SmallTreeQuat64Ext:
            default:
                throw new InvalidOperationException();
        }
    }

    public int WrittenSize => Format switch {
        VectorCompressionFormat.NoCompressVec3 => Data.Length * 12,
        VectorCompressionFormat.NoCompress => throw new NotSupportedException(),
        VectorCompressionFormat.NoCompressQuat => throw new NotSupportedException(),
        VectorCompressionFormat.ShortInt3Quat => throw new NotSupportedException(),
        VectorCompressionFormat.SmallTreeQuat32 => throw new NotSupportedException(),
        VectorCompressionFormat.SmallTreeQuat48 => throw new NotSupportedException(),
        VectorCompressionFormat.SmallTreeQuat64 => throw new NotSupportedException(),
        VectorCompressionFormat.PolarQuat => throw new NotSupportedException(),
        VectorCompressionFormat.SmallTreeQuat64Ext => throw new NotSupportedException(),
        _ => throw new ArgumentOutOfRangeException(nameof(Format), Format, null),
    };

    public override string ToString() => $"{nameof(ControllerKeyPosition)}<{Format}>: {Data.Length} items";
}
