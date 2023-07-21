using System;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SynergyLib.Util;

public static class MiscUtils {
    public static Vector4 NormalizeSNorm(this Bgra32 color) => new(
        float.Clamp((color.R - 127) / 127f, -1f, 1f),
        float.Clamp((color.G - 127) / 127f, -1f, 1f),
        float.Clamp((color.B - 127) / 127f, -1f, 1f),
        float.Clamp((color.A - 127) / 127f, -1f, 1f));
    
    public static Vector4 SampleNormalized(
        this Image<Bgra32> image,
        float u,
        float v,
        TileMethod tileU,
        TileMethod tileV) {
        u = tileU switch {
            TileMethod.Repeat => u > 0 ? u % 1 : 1 + u % 1,
            TileMethod.Mirror => MathF.Abs(1f - MathF.Abs(u) % 2),
            TileMethod.Clamp => float.Clamp(u, 0f, 1f),
            _ => throw new ArgumentOutOfRangeException(nameof(tileU), tileU, null),
        };
        v = tileV switch {
            TileMethod.Repeat => v > 0 ? v % 1 : 1 + v % 1,
            TileMethod.Mirror => MathF.Abs(v) % 1,
            TileMethod.Clamp => float.Clamp(v, 0f, 1f),
            _ => throw new ArgumentOutOfRangeException(nameof(tileU), tileU, null),
        };

        u *= image.Width;
        v *= image.Height;
        return image[(int) u, (int) v].NormalizeSNorm();
    }

    public enum TileMethod {
        Repeat,
        Mirror,
        Clamp,
    }
}
