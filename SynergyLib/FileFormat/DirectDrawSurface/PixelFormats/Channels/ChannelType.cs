using System;

namespace SynergyLib.FileFormat.DirectDrawSurface.PixelFormats.Channels;

[Flags]
public enum ChannelType : byte {
    Typeless = 0,

    Signed = 1,
    Unsigned = 2,

    Integer = 4,
    FloatingPoint = 8,

    Normalized = 16,
    Srgb = 32,

    Snorm = Signed | Normalized | Integer,
    Unorm = Unsigned | Normalized | Integer,
    UnormSrgb = Unorm | Srgb,
    Sint = Signed | Integer,
    Uint = Unsigned | Integer,

    // Standard FP32.
    Float = FloatingPoint | Normalized,

    // Standard FP16.
    Half = Signed | FloatingPoint,

    // https://learn.microsoft.com/en-us/windows/win32/direct3d11/bc6h-format
    // 1 sign bit + 5 exponent bits + 10 mantissa bits = FP16.
    Sf16 = Half,

    // 5 exponent bits + 11 mantissa bits
    Uf16 = Unsigned | FloatingPoint,
}
