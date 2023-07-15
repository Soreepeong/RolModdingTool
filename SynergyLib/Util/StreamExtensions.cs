using System;
using System.IO;

namespace SynergyLib.Util;

public static class StreamExtensions {
    public static byte ReadByteOrThrow(this Stream stream) => stream.ReadByte() switch {
        -1 => throw new EndOfStreamException(),
        var r => (byte) r
    };

    public static void CopyToLength(this Stream source, Stream destination, int length) {
        Span<byte> buffer = stackalloc byte[4096];
        while (length > 0) {
            var chunk = source.Read(buffer[..Math.Min(buffer.Length, length)]);
            if (chunk == 0)
                throw new EndOfStreamException();
            destination.Write(buffer[..chunk]);
            length -= chunk;
        }
    }
}
