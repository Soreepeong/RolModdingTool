using System;
using System.Numerics;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Enums;
using WiiUStreamTool.Util.BinaryRW;
using WiiUStreamTool.Util.MathExtras;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;

public struct ControllerKeyPosition {
    public CompressionFormat Format;
    public Vector3[] Data = Array.Empty<Vector3>();

    public ControllerKeyPosition() { }

    public void ReadFrom(NativeReader b, CompressionFormat format, int length) {
        var data = new Vector3[length];
        switch (format) {
            case CompressionFormat.NoCompressVec3:
                for (var i = 0; i < length; i++)
                    data[i] = b.ReadVector3();
                break;
            case CompressionFormat.NoCompress:
            case CompressionFormat.NoCompressQuat:
            case CompressionFormat.ShortInt3Quat:
            case CompressionFormat.SmallTreeQuat32:
            case CompressionFormat.SmallTreeQuat48:
            case CompressionFormat.SmallTreeQuat64:
            case CompressionFormat.PolarQuat:
            case CompressionFormat.SmallTreeQuat64Ext:
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        Format = format;
        Data = data;
    }

    public void WriteTo(NativeWriter w) {
        switch (Format) {
            case CompressionFormat.NoCompressVec3:
                foreach (var v in Data)
                    w.Write(v);
                break;
            case CompressionFormat.NoCompress:
            case CompressionFormat.NoCompressQuat:
            case CompressionFormat.ShortInt3Quat:
            case CompressionFormat.SmallTreeQuat32:
            case CompressionFormat.SmallTreeQuat48:
            case CompressionFormat.SmallTreeQuat64:
            case CompressionFormat.PolarQuat:
            case CompressionFormat.SmallTreeQuat64Ext:
            default:
                throw new InvalidOperationException();
        }

    }

    public int WrittenSize => Format switch {
        CompressionFormat.NoCompressVec3 => Data.Length * 12,
        CompressionFormat.NoCompress => throw new NotSupportedException(),
        CompressionFormat.NoCompressQuat => throw new NotSupportedException(),
        CompressionFormat.ShortInt3Quat => throw new NotSupportedException(),
        CompressionFormat.SmallTreeQuat32 => throw new NotSupportedException(),
        CompressionFormat.SmallTreeQuat48 => throw new NotSupportedException(),
        CompressionFormat.SmallTreeQuat64 => throw new NotSupportedException(),
        CompressionFormat.PolarQuat => throw new NotSupportedException(),
        CompressionFormat.SmallTreeQuat64Ext => throw new NotSupportedException(),
        _ => throw new ArgumentOutOfRangeException(nameof(Format), Format, null),
    };

    public override string ToString() => $"{nameof(ControllerKeyPosition)}<{Format}>: {Data.Length} items";
}
