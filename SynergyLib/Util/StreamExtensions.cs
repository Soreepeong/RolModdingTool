using System;
using System.IO;
using System.Threading;

namespace SynergyLib.Util;

public static class StreamExtensions {
    public static byte ReadByteOrThrow(this Stream stream) => stream.ReadByte() switch {
        -1 => throw new EndOfStreamException(),
        var r => (byte) r
    };

    public static void CopyToLength(this Stream source, Stream destination, int length, CancellationToken cancellationToken) {
        Span<byte> buffer = stackalloc byte[4096];
        while (length > 0) {
            cancellationToken.ThrowIfCancellationRequested();

            var chunk = source.Read(buffer[..Math.Min(buffer.Length, length)]);
            if (chunk == 0)
                throw new EndOfStreamException();
            destination.Write(buffer[..chunk]);
            length -= chunk;
        }
    }
}
