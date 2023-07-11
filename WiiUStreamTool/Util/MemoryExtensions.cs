using System;

namespace WiiUStreamTool.Util;

public static class MemoryExtensions {
    public static bool StartsWith<T>(this Memory<T> memory, ReadOnlySpan<T> sequence) where T : IEquatable<T> =>
        memory.Span.StartsWith(sequence);
    
    public static unsafe T GetNativeStruct<T>(this Span<byte> span, int structIndex = 0) where T : unmanaged {
        if (structIndex < 0 || structIndex >= span.Length / sizeof(T))
            throw new ArgumentOutOfRangeException(nameof(structIndex), structIndex, null);
        
        fixed (void* p = span)
            return ((T*) p)[structIndex];
    }

    public static unsafe void SetNativeStruct<T>(this Span<byte> span, in T data, int structIndex = 0)
        where T : unmanaged {
        if (structIndex < 0 || structIndex >= span.Length / sizeof(T))
            throw new ArgumentOutOfRangeException(nameof(structIndex), structIndex, null);

        fixed (void* p = span)
            ((T*) p)[structIndex] = data;
    }
}