using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
            dds.SliceOrFaceSpan(imageIndex, mipmapIndex, sliceIndex),
            dds.Pitch(mipmapIndex),
            w,
            h);
        return Image.LoadPixelData<Bgra32>(buffer, w, h);
    }

    public static async Task<DdsFile> ToDdsFile2D(
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
        legacyHeader.WriteTo(dds.Data);

        Memory<Bgra32> memory1 = new Bgra32[dds.Header.Width * dds.Header.Height];
        image.CopyPixelDataTo(memory1.Span);
        Memory<Bgra32> memory2 = new Bgra32[memory1.Length / 4];

        var prevHeight = 0;
        var prevWidth = 0;
            
        void NextMipmap(int mipHeight, int mipWidth) {
            var buffer = memory1.Span;
            var mipmapBuffer = memory2.Span;
            if (prevHeight != mipHeight && prevWidth != mipWidth) {
                var loopH = prevHeight % 2 == 0 ? mipHeight : mipHeight - 1;
                for (var y = 0; y < loopH; y++) {
                    squishOptions.CancellationToken.ThrowIfCancellationRequested();

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
                    squishOptions.CancellationToken.ThrowIfCancellationRequested();

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
                    squishOptions.CancellationToken.ThrowIfCancellationRequested();

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
            
        for (var mipIndex = 0; mipIndex < numMips; mipIndex++) {
            squishOptions.CancellationToken.ThrowIfCancellationRequested();

            var mipHeight = dds.Height(mipIndex);
            var mipWidth = dds.Width(mipIndex);
            if (mipIndex != 0) {
                (memory1, memory2) = (memory2[..(mipHeight * mipWidth)], memory1);
                NextMipmap(mipHeight, mipWidth);
            }

            await Squish.CompressImageAsync<Bgra32>(
                memory1,
                4 * mipWidth,
                mipWidth,
                mipHeight,
                dds.SliceOrFaceMemory(0, mipIndex, 0),
                squishOptions);

            prevHeight = mipHeight;
            prevWidth = mipWidth;
        }

        return dds;
    }
}
