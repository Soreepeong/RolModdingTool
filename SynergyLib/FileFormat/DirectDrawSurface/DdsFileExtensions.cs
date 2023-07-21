using System;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SynergyLib.FileFormat.DotSquish;

namespace SynergyLib.FileFormat.DirectDrawSurface;

public static class DdsFileExtensions {
    public static Image<Bgra32> ToImageBgra32(
        this DdsFile dds,
        int imageIndex,
        int mipmapIndex,
        int sliceIndex) {
        var w = dds.Width(mipmapIndex);
        var h = dds.Height(mipmapIndex);
        var buffer = new byte[w * h * 4];
        dds.PixelFormat.ToB8G8R8A8(
            buffer.AsSpan(),
            w * 4,
            dds.SliceOrFaceData(imageIndex, mipmapIndex, sliceIndex),
            dds.Pitch(mipmapIndex),
            w,
            h);
        return Image.LoadPixelData<Bgra32>(buffer, w, h);
    }

    public static DdsFile ToDdsFile2D(
        this Image<Bgra32> image,
        string name,
        SquishOptions squishOptions,
        byte[]? tail,
        int maxMipmaps) {
        var legacyHeader = new DdsHeaderLegacy {
            Magic = DdsHeaderLegacy.MagicValue,
            Header = new() {
                Size = Unsafe.SizeOf<DdsHeader>(),
                Flags = DdsHeaderFlags.Caps |
                    DdsHeaderFlags.Height |
                    DdsHeaderFlags.Width |
                    DdsHeaderFlags.PixelFormat,
                Height = image.Height,
                Width = image.Width,
                LinearSize =
                    Math.Max(1, (image.Width + 3) / 4) *
                    Math.Max(1, (image.Height + 3) / 4) *
                    (squishOptions.Method == SquishMethod.Dxt1 ? 8 : 16),
                Caps = DdsCaps1.Texture,
                PixelFormat = new() {
                    Size = Unsafe.SizeOf<DdsPixelFormat>(),
                    Flags = DdsPixelFormatFlags.FourCc,
                    FourCc = squishOptions.Method switch {
                        SquishMethod.Dxt1 => DdsFourCc.Dxt1,
                        SquishMethod.Dxt3 => DdsFourCc.Dxt3,
                        SquishMethod.Dxt5 => DdsFourCc.Dxt5,
                        _ => throw new ArgumentOutOfRangeException(nameof(squishOptions), squishOptions, null),
                    },
                },
            },
        };

        var numMips = 0;
        for (var n = Math.Max(image.Width, image.Height); n > 0; n >>= 1)
            numMips++;
        numMips = Math.Max(1, Math.Min(numMips, maxMipmaps));
        if (numMips >= 2) {
            legacyHeader.Header.Caps |= DdsCaps1.Complex | DdsCaps1.Mipmap;
            legacyHeader.Header.Flags |= DdsHeaderFlags.MipmapCount;
            legacyHeader.Header.MipMapCount = numMips;
        }

        var dds = new DdsFile(name, legacyHeader, null, Array.Empty<byte>());
        dds.SetBufferUnchecked(new byte[dds.DataOffset + dds.ImageSize + tail?.Length ?? 0]);
        tail?.CopyTo(dds.Data[(dds.DataOffset + dds.ImageSize)..]);
        unsafe {
            var s = new Span<byte>(&legacyHeader, Unsafe.SizeOf<DdsHeaderLegacy>());
            s.CopyTo(dds.Data);
        }

        Span<Bgra32> buffer = new Bgra32[dds.Header.Width * dds.Header.Height];
        image.CopyPixelDataTo(buffer);
        Span<Bgra32> mipmapBuffer = new Bgra32[buffer.Length / 4];

        unsafe {
            var prevHeight = 0;
            var prevWidth = 0;
            for (var mipIndex = 0; mipIndex < numMips; mipIndex++) {
                var mipHeight = dds.Height(mipIndex);
                var mipWidth = dds.Width(mipIndex);
                if (mipIndex != 0) {
                    // Span<...> unsupported as Tuple type argument
                    var tmp = buffer;
                    buffer = mipmapBuffer[..(mipHeight * mipWidth)];
                    mipmapBuffer = tmp;

                    if (prevHeight != mipHeight && prevWidth != mipWidth) {
                        var loopH = prevHeight % 2 == 0 ? mipHeight : mipHeight - 1;
                        for (var y = 0; y < loopH; y++) {
                            var loopW = prevWidth % 2 == 0 ? mipWidth : mipWidth - 1;
                            for (var x = 0; x < loopW; x++) {
                                buffer[y * mipWidth + x].FromVector4(
                                    (
                                        mipmapBuffer[(y * 2 + 0) * prevWidth + x * 2 + 0].ToVector4() +
                                        mipmapBuffer[(y * 2 + 0) * prevWidth + x * 2 + 1].ToVector4() +
                                        mipmapBuffer[(y * 2 + 1) * prevWidth + x * 2 + 0].ToVector4() +
                                        mipmapBuffer[(y * 2 + 1) * prevWidth + x * 2 + 1].ToVector4()) / 4);
                            }

                            if (loopW != mipWidth) {
                                var x = mipWidth - 1;
                                buffer[y * mipWidth + x].FromVector4(
                                    (
                                        mipmapBuffer[(y * 2 + 0) * prevWidth + x * 2 + 0].ToVector4() +
                                        mipmapBuffer[(y * 2 + 1) * prevWidth + x * 2 + 0].ToVector4()) / 2);
                            }
                        }

                        if (loopH != mipHeight) {
                            var loopW = prevWidth % 2 == 0 ? mipWidth : mipWidth - 1;
                            var y = mipHeight - 1;
                            for (var x = 0; x < loopW; x++) {
                                buffer[y * mipWidth + x].FromVector4(
                                    (
                                        mipmapBuffer[(y * 2 + 0) * prevWidth + x * 2 + 0].ToVector4() +
                                        mipmapBuffer[(y * 2 + 0) * prevWidth + x * 2 + 1].ToVector4()) / 2);
                            }

                            if (loopW != mipWidth) {
                                var x = mipWidth - 1;
                                buffer[y * mipWidth + x] = mipmapBuffer[(y * 2 + 0) * prevWidth + x * 2 + 0];
                            }
                        }
                    } else if (prevHeight != mipHeight) {
                        var loopH = prevHeight % 2 == 0 ? mipHeight : mipHeight - 1;
                        for (var y = 0; y < loopH; y++) {
                            for (var x = 0; x < prevWidth; x++) {
                                buffer[y * mipWidth + x].FromVector4(
                                    (
                                        mipmapBuffer[(y * 2 + 0) * prevWidth + x + 0].ToVector4() +
                                        mipmapBuffer[(y * 2 + 1) * prevWidth + x + 0].ToVector4()) / 2);
                            }
                        }

                        if (loopH != mipHeight) {
                            var y = mipHeight - 1;
                            for (var x = 0; x < prevWidth; x++) {
                                buffer[y * mipWidth + x] = mipmapBuffer[(y * 2 + 0) * prevWidth + x + 0];
                            }
                        }
                    } else if (prevWidth != mipWidth) {
                        for (var y = 0; y < mipHeight; y++) {
                            var loopW = prevWidth % 2 == 0 ? mipWidth : mipWidth - 1;
                            for (var x = 0; x < loopW; x++) {
                                buffer[y * mipWidth + x].FromVector4(
                                    (
                                        mipmapBuffer[(y * 2 + 0) * prevWidth + x * 2 + 0].ToVector4() +
                                        mipmapBuffer[(y * 2 + 0) * prevWidth + x * 2 + 1].ToVector4()) / 2);
                            }

                            if (loopW != mipWidth) {
                                var x = mipWidth - 1;
                                buffer[y * mipWidth + x] = mipmapBuffer[(y * 2 + 0) * prevWidth + x * 2 + 0];
                            }
                        }
                    }
                }

                prevHeight = mipHeight;
                prevWidth = mipWidth;

                fixed (void* p = buffer) {
                    var bufferByteSpan = new Span<byte>(p, mipHeight * mipWidth * Unsafe.SizeOf<Bgra32>());
                    Squish.CompressImage(
                        bufferByteSpan,
                        4 * mipWidth,
                        mipWidth,
                        mipHeight,
                        dds.SliceOrFaceData(0, mipIndex, 0),
                        squishOptions);
                }
            }
        }

        return dds;
    }
}
