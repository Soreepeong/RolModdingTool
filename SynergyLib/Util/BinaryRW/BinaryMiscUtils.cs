using System;

namespace SynergyLib.Util.BinaryRW; 

public static class BinaryMiscUtils {
    public static unsafe T[] GetBigEndianArray<T>(ReadOnlySpan<byte> arr) where T : unmanaged {
        var res = new T[arr.Length / sizeof(T)];
        fixed (byte* p = arr)
            new Span<T>(p, res.Length).CopyTo(res);
        if (BitConverter.IsLittleEndian && sizeof(T) > 1) {
            fixed (T* r2 = res) {
                for (var i = 0; i < res.Length; i++) {
                    new Span<byte>((byte*) &r2[i], sizeof(T)).Reverse();
                }
            }
        }
        return res;
    }
}
