using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats.Channels;

namespace SynergyLib.FileFormat.DirectDrawSurface;

public class DdsFile {
    public readonly DdsHeaderLegacy LegacyHeader;
    public readonly bool UseDxt10Header;
    public readonly DdsHeaderDxt10 Dxt10Header;

    private byte[] _data;

    public DdsFile(string name, DdsHeaderLegacy legacyHeader, DdsHeaderDxt10? dxt10Header, byte[] data) {
        Name = name;
        LegacyHeader = legacyHeader;
        UseDxt10Header = dxt10Header is not null;
        Dxt10Header = dxt10Header ?? new();
        _data = data;
    }

    public DdsFile(string name, Stream stream, bool leaveOpen = false) {
        Name = name;
        try {
            try {
                _data = new byte[stream.Length];
                stream.ReadExactly(_data);
            } catch (NotSupportedException) {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                _data = ms.ToArray();
            }

            unsafe {
                fixed (void* lh = &LegacyHeader)
                    Marshal.Copy(_data, 0, (nint) lh, sizeof(DdsHeaderLegacy));
                
                if (LegacyHeader.Header.PixelFormat.Flags.HasFlag(DdsPixelFormatFlags.FourCc) &&
                    LegacyHeader.Header.PixelFormat.FourCc == DdsFourCc.Dx10) {
                    UseDxt10Header = true;
                    fixed (void* dh = &Dxt10Header)
                        Marshal.Copy(_data, sizeof(DdsHeaderLegacy), (nint) dh, sizeof(DdsHeaderDxt10));
                }
            }
        } finally {
            if (!leaveOpen)
                stream.Dispose();
        }
    }

    public DdsFile(string name, byte[] data) {
        Name = name;
        _data = data;
        unsafe {
            fixed (void* lh = &LegacyHeader)
                Marshal.Copy(_data, 0, (nint) lh, sizeof(DdsHeaderLegacy));
                
            if (LegacyHeader.Header.PixelFormat.Flags.HasFlag(DdsPixelFormatFlags.FourCc) &&
                LegacyHeader.Header.PixelFormat.FourCc == DdsFourCc.Dx10) {
                UseDxt10Header = true;
                fixed (void* dh = &Dxt10Header)
                    Marshal.Copy(_data, sizeof(DdsHeaderLegacy), (nint) dh, sizeof(DdsHeaderDxt10));
            }
        }
    }

    public string Name { get; }

    public int DataOffset =>
        Unsafe.SizeOf<DdsHeaderLegacy>() +
        (UseDxt10Header ? Unsafe.SizeOf<DdsHeaderDxt10>() : 0);

    public MemoryStream CreateStream() => new(_data, false);

    public DdsHeader Header => LegacyHeader.Header;

    public Span<byte> Body => new(_data, DataOffset, _data.Length - DataOffset);

    public Span<byte> Data => _data.AsSpan();

    public int NumImages => UseDxt10Header ? Dxt10Header.ArraySize : 1;

    public int NumMipmaps => Header.Flags.HasFlag(DdsHeaderFlags.MipmapCount) ? Header.MipMapCount : 1;

    public int Bpp => PixelFormat.Bpp;

    public bool Is1D => !IsCubeMap && (Header.Flags & DdsHeaderFlags.DimensionMask) == DdsHeaderFlags.Dimension1;
    
    public bool Is2D => !IsCubeMap && (Header.Flags & DdsHeaderFlags.DimensionMask) == DdsHeaderFlags.Dimension2;
    
    public bool Is3D => !IsCubeMap && (Header.Flags & DdsHeaderFlags.DimensionMask) == DdsHeaderFlags.Dimension3;

    public bool IsCubeMap => Header.Caps2.HasFlag(DdsCaps2.Cubemap);

    public int Width(int mipmapIndex) =>
        0 <= mipmapIndex && mipmapIndex < NumMipmaps
            ? Header.Flags.HasFlag(DdsHeaderFlags.Width) ? Math.Max(1, Header.Width >> mipmapIndex) : 1
            : throw new ArgumentOutOfRangeException(nameof(mipmapIndex), mipmapIndex, null);

    public int Pitch(int mipmapIndex) {
        var pf = PixelFormat;
        if (pf is BcPixelFormat bcPixelFormat)
            return Math.Max(1, (Width(mipmapIndex) + 3) / 4) * bcPixelFormat.BlockSize;

        // For R8G8_B8G8, G8R8_G8B8, legacy UYVY-packed, and legacy YUY2-packed formats, compute the pitch as:
        // ((width+1) >> 1) * 4

        return (Width(mipmapIndex) * pf.Bpp + 7) / 8;
    }

    public int Height(int mipmapIndex) =>
        0 <= mipmapIndex && mipmapIndex < NumMipmaps
            ? Header.Flags.HasFlag(DdsHeaderFlags.Height) ? Math.Max(1, Header.Height >> mipmapIndex) : 1
            : throw new ArgumentOutOfRangeException(nameof(mipmapIndex), mipmapIndex, null);

    public int NumFaces => !IsCubeMap
        ? 1
        : (Header.Caps2.HasFlag(DdsCaps2.CubemapNegativeX) ? 1 : 0)
        + (Header.Caps2.HasFlag(DdsCaps2.CubemapPositiveX) ? 1 : 0)
        + (Header.Caps2.HasFlag(DdsCaps2.CubemapNegativeY) ? 1 : 0)
        + (Header.Caps2.HasFlag(DdsCaps2.CubemapPositiveY) ? 1 : 0)
        + (Header.Caps2.HasFlag(DdsCaps2.CubemapNegativeZ) ? 1 : 0)
        + (Header.Caps2.HasFlag(DdsCaps2.CubemapPositiveZ) ? 1 : 0);

    public int Depth(int mipmapIndex) => 0 <= mipmapIndex && mipmapIndex < NumMipmaps
        ? Header.Flags.HasFlag(DdsHeaderFlags.Depth) ? Math.Max(1, Header.Depth >> mipmapIndex) : 1
        : throw new ArgumentOutOfRangeException(nameof(mipmapIndex), mipmapIndex, null);

    public int DepthOrNumFaces(int mipmapIndex) => IsCubeMap ? NumFaces : Depth(mipmapIndex);

    public int SliceSize(int mipmapIndex) {
        var pf = PixelFormat;
        if (pf is BcPixelFormat bcPixelFormat) {
            return Math.Max(1, (Width(mipmapIndex) + 3) / 4) *
                Math.Max(1, (Height(mipmapIndex) + 3) / 4) *
                bcPixelFormat.BlockSize;
        }

        // For R8G8_B8G8, G8R8_G8B8, legacy UYVY-packed, and legacy YUY2-packed formats, compute the pitch as:
        // ((width+1) >> 1) * 4

        return (Width(mipmapIndex) * pf.Bpp + 7) / 8 * Height(mipmapIndex);
    }

    public int MipmapSize(int mipmapIndex) => SliceSize(mipmapIndex) * Depth(mipmapIndex);

    public int FaceSize => Enumerable.Range(0, NumMipmaps).Sum(MipmapSize);

    public int ImageSize => FaceSize * NumFaces;

    public int ImageDataOffset(int imageIndex, out int size) {
        if (imageIndex < 0 || imageIndex >= NumImages)
            throw new ArgumentOutOfRangeException(nameof(imageIndex), imageIndex, null);

        size = ImageSize;
        return DataOffset + size * imageIndex;
    }

    public int FaceDataOffset(int imageIndex, int faceIndex, out int size) {
        var offset = ImageDataOffset(imageIndex, out _);
        size = FaceSize;
        return offset + size * faceIndex;
    }

    public int MipmapDataOffset(int imageIndex, int faceIndex, int mipmapIndex, out int size) {
        var baseOffset = FaceDataOffset(imageIndex, faceIndex, out _);
        var mipOffset = Enumerable.Range(0, mipmapIndex).Sum(MipmapSize);
        size = MipmapSize(mipmapIndex);
        return baseOffset + mipOffset;
    }

    public int SliceDataOffset(int imageIndex, int faceIndex, int mipmapIndex, int sliceIndex, out int size) {
        var offset = MipmapDataOffset(imageIndex, faceIndex, mipmapIndex, out _);
        size = SliceSize(mipmapIndex);
        return offset + size * sliceIndex;
    }

    public int SliceOrFaceDataOffset(int imageIndex, int mipmapIndex, int sliceIndex, out int size) => IsCubeMap
        ? SliceDataOffset(imageIndex, sliceIndex, mipmapIndex, 0, out size)
        : SliceDataOffset(imageIndex, 0, mipmapIndex, sliceIndex, out size);

    public Span<byte> ImageSpan(int imageIndex) {
        var offset = ImageDataOffset(imageIndex, out var size);
        return new(_data, offset, size);
    }

    public Span<byte> FaceSpan(int imageIndex, int faceIndex) {
        var offset = FaceDataOffset(imageIndex, faceIndex, out var size);
        return new(_data, offset, size);
    }

    public Span<byte> MipmapSpan(int imageIndex, int faceIndex, int mipmapIndex) {
        var offset = MipmapDataOffset(imageIndex, faceIndex, mipmapIndex, out var size);
        return new(_data, offset, size);
    }

    public Span<byte> SliceSpan(int imageIndex, int faceIndex, int mipmapIndex, int sliceIndex) {
        var offset = SliceDataOffset(imageIndex, faceIndex, mipmapIndex, sliceIndex, out var size);
        return new(_data, offset, size);
    }

    public Span<byte> SliceOrFaceSpan(int imageIndex, int mipmapIndex, int sliceIndex) => IsCubeMap
        ? SliceSpan(imageIndex, sliceIndex, mipmapIndex, 0)
        : SliceSpan(imageIndex, 0, mipmapIndex, sliceIndex);

    public Memory<byte> ImageMemory(int imageIndex) {
        var offset = ImageDataOffset(imageIndex, out var size);
        return new(_data, offset, size);
    }

    public Memory<byte> FaceMemory(int imageIndex, int faceIndex) {
        var offset = FaceDataOffset(imageIndex, faceIndex, out var size);
        return new(_data, offset, size);
    }

    public Memory<byte> MipmapMemory(int imageIndex, int faceIndex, int mipmapIndex) {
        var offset = MipmapDataOffset(imageIndex, faceIndex, mipmapIndex, out var size);
        return new(_data, offset, size);
    }

    public Memory<byte> SliceMemory(int imageIndex, int faceIndex, int mipmapIndex, int sliceIndex) {
        var offset = SliceDataOffset(imageIndex, faceIndex, mipmapIndex, sliceIndex, out var size);
        return new(_data, offset, size);
    }

    public Memory<byte> SliceOrFaceMemory(int imageIndex, int mipmapIndex, int sliceIndex) => IsCubeMap
        ? SliceMemory(imageIndex, sliceIndex, mipmapIndex, 0)
        : SliceMemory(imageIndex, 0, mipmapIndex, sliceIndex);

    public void SetBufferUnchecked(byte[] buffer) => _data = buffer;

    public PixelFormat PixelFormat {
        get {
            var pf = Header.PixelFormat;

            if (!pf.Flags.HasFlag(DdsPixelFormatFlags.FourCc)) {
                var alpha = ChannelDefinition.Empty;

                if (pf.Flags.HasFlag(DdsPixelFormatFlags.AlphaPixels))
                    alpha = ChannelDefinition.FromMask(ChannelType.Unorm, pf.ABitMask);

                if (pf.Flags.HasFlag(DdsPixelFormatFlags.Rgb)) {
                    var xbitmask =
                        unchecked((1u << pf.RgbBitCount) - 1u) & ~(pf.RBitMask | pf.GBitMask | pf.BBitMask) &
                        (pf.Flags.HasFlag(DdsPixelFormatFlags.AlphaPixels) ? ~pf.ABitMask : ~0u);
                    return new RgbaPixelFormat(
                        alpha.IsEmpty ? AlphaType.None : AlphaType.Straight,
                        r: ChannelDefinition.FromMask(ChannelType.Unorm, pf.RBitMask),
                        g: ChannelDefinition.FromMask(ChannelType.Unorm, pf.GBitMask),
                        b: ChannelDefinition.FromMask(ChannelType.Unorm, pf.BBitMask),
                        a: alpha,
                        x1: ChannelDefinition.FromMask(ChannelType.Typeless, xbitmask));
                }

                if (pf.Flags.HasFlag(DdsPixelFormatFlags.Yuv)) {
                    var xbitmask =
                        unchecked((1u << pf.RgbBitCount) - 1u) & ~(pf.RBitMask | pf.GBitMask | pf.BBitMask) &
                        (pf.Flags.HasFlag(DdsPixelFormatFlags.AlphaPixels) ? ~pf.ABitMask : ~0u);
                    return new YuvPixelFormat(
                        alpha.IsEmpty ? AlphaType.None : AlphaType.Straight,
                        y: ChannelDefinition.FromMask(ChannelType.Unorm, pf.RBitMask),
                        u: ChannelDefinition.FromMask(ChannelType.Unorm, pf.GBitMask),
                        v: ChannelDefinition.FromMask(ChannelType.Unorm, pf.BBitMask),
                        a: alpha,
                        x: ChannelDefinition.FromMask(ChannelType.Typeless, xbitmask));
                }

                if (pf.Flags.HasFlag(DdsPixelFormatFlags.Luminance)) {
                    var xbitmask =
                        unchecked((1u << pf.RgbBitCount) - 1u) & ~pf.RBitMask &
                        (pf.Flags.HasFlag(DdsPixelFormatFlags.AlphaPixels) ? ~pf.ABitMask : ~0u);
                    return new LumiPixelFormat(
                        alpha.IsEmpty ? AlphaType.None : AlphaType.Straight,
                        l: ChannelDefinition.FromMask(ChannelType.Unorm, pf.RBitMask),
                        a: alpha,
                        x: ChannelDefinition.FromMask(ChannelType.Typeless, xbitmask));
                }

                if (pf.Flags.HasFlag(DdsPixelFormatFlags.Alpha)) {
                    var xbitmask = unchecked((1u << pf.RgbBitCount) - 1u) & ~pf.ABitMask;
                    return new RgbaPixelFormat(
                        AlphaType.Straight,
                        a: alpha,
                        x1: ChannelDefinition.FromMask(ChannelType.Typeless, xbitmask));
                }

                return UnknownPixelFormat.Instance;
            }

            var ipf = PixelFormatResolver.GetPixelFormat(pf.FourCc);
            if (!Equals(ipf, UnknownPixelFormat.Instance))
                return ipf;

            if (pf.FourCc != DdsFourCc.Dx10 || !UseDxt10Header)
                return UnknownPixelFormat.Instance;

            return PixelFormatResolver.GetPixelFormat(Dxt10Header.MiscFlags2 switch {
                DdsHeaderDxt10MiscFlags2.AlphaModeUnknown => AlphaType.Straight,
                DdsHeaderDxt10MiscFlags2.AlphaModeStraight => AlphaType.Straight,
                DdsHeaderDxt10MiscFlags2.AlphaModePremultiplied => AlphaType.Premultiplied,
                DdsHeaderDxt10MiscFlags2.AlphaModeOpaque => AlphaType.None,
                DdsHeaderDxt10MiscFlags2.AlphaModeCustom => AlphaType.Custom,
                _ => throw new ArgumentOutOfRangeException()
            }, Dxt10Header.DxgiFormat);
        }
    }
}