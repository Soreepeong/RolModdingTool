using System.Collections.Generic;
using System.Linq;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats.Channels;

namespace SynergyLib.FileFormat.DirectDrawSurface.PixelFormats;

public static class PixelFormatResolver {
    public static readonly IReadOnlyDictionary<DdsFourCc, PixelFormat> FourCcToPixelFormat;

    public static readonly IReadOnlyDictionary<AlphaType, IReadOnlyDictionary<DxgiFormat, PixelFormat>>
        DxgiFormatToPixelFormat;

    // https://learn.microsoft.com/en-us/windows/win32/direct3d10/d3d10-graphics-programming-guide-resources-data-conversion
    static PixelFormatResolver() {
        FourCcToPixelFormat = new Dictionary<DdsFourCc, PixelFormat> {
            {DdsFourCc.Dxt1, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 1)},
            {DdsFourCc.Dxt2, new BcPixelFormat(ChannelType.Unorm, AlphaType.Premultiplied, 2)},
            {DdsFourCc.Dxt3, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 2)},
            {DdsFourCc.Dxt4, new BcPixelFormat(ChannelType.Unorm, AlphaType.Premultiplied, 3)},
            {DdsFourCc.Dxt5, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 3)},
            {DdsFourCc.Bc4, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 4)},
            {DdsFourCc.Bc4U, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 4)},
            {DdsFourCc.Bc4S, new BcPixelFormat(ChannelType.Snorm, AlphaType.Straight, 4)},
            {DdsFourCc.Bc5, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 5)},
            {DdsFourCc.Bc5U, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 5)},
            {DdsFourCc.Bc5S, new BcPixelFormat(ChannelType.Snorm, AlphaType.Straight, 5)},
        };
        DxgiFormatToPixelFormat =
            new Dictionary<AlphaType, IReadOnlyDictionary<DxgiFormat, PixelFormat>> {
                {
                    AlphaType.None, new Dictionary<DxgiFormat, PixelFormat> {
                        {
                            DxgiFormat.R32G32B32Typeless,
                            RgbaPixelFormat.NewRgb(32, 32, 32, 0, 0, ChannelType.Typeless, AlphaType.None)
                        }, {
                            DxgiFormat.R32G32B32Float,
                            RgbaPixelFormat.NewRgb(32, 32, 32, 0, 0, ChannelType.Float, AlphaType.None)
                        }, {
                            DxgiFormat.R32G32B32Uint,
                            RgbaPixelFormat.NewRgb(32, 32, 32, 0, 0, ChannelType.Uint, AlphaType.None)
                        }, {
                            DxgiFormat.R32G32B32Sint,
                            RgbaPixelFormat.NewRgb(32, 32, 32, 0, 0, ChannelType.Sint, AlphaType.None)
                        }, {
                            DxgiFormat.R32G32Float,
                            RgbaPixelFormat.NewRg(32, 32, 0, 0, ChannelType.Float, AlphaType.None)
                        },
                        {DxgiFormat.R32G32Uint, RgbaPixelFormat.NewRg(32, 32, 0, 0, ChannelType.Uint, AlphaType.None)},
                        {DxgiFormat.R32G32Sint, RgbaPixelFormat.NewRg(32, 32, 0, 0, ChannelType.Sint, AlphaType.None)}, {
                            DxgiFormat.R16G16Typeless,
                            RgbaPixelFormat.NewRg(16, 16, 0, 0, ChannelType.Typeless, AlphaType.None)
                        }, {
                            DxgiFormat.R16G16Float,
                            RgbaPixelFormat.NewRg(16, 16, 0, 0, ChannelType.Float, AlphaType.None)
                        }, {
                            DxgiFormat.R16G16Unorm,
                            RgbaPixelFormat.NewRg(16, 16, 0, 0, ChannelType.Unorm, AlphaType.None)
                        },
                        {DxgiFormat.R16G16Uint, RgbaPixelFormat.NewRg(16, 16, 0, 0, ChannelType.Uint, AlphaType.None)}, {
                            DxgiFormat.R16G16Snorm,
                            RgbaPixelFormat.NewRg(16, 16, 0, 0, ChannelType.Snorm, AlphaType.None)
                        },
                        {DxgiFormat.R16G16Sint, RgbaPixelFormat.NewRg(16, 16, 0, 0, ChannelType.Sint, AlphaType.None)},
                        {DxgiFormat.R32Typeless, RgbaPixelFormat.NewR(32, 0, 0, ChannelType.Typeless, AlphaType.None)},
                        {DxgiFormat.R32Float, RgbaPixelFormat.NewR(32, 0, 0, ChannelType.Float, AlphaType.None)},
                        {DxgiFormat.R32Uint, RgbaPixelFormat.NewR(32, 0, 0, ChannelType.Uint, AlphaType.None)},
                        {DxgiFormat.R32Sint, RgbaPixelFormat.NewR(32, 0, 0, ChannelType.Sint, AlphaType.None)}, {
                            DxgiFormat.R24G8Typeless,
                            RgbaPixelFormat.NewRg(24, 8, 0, 0, ChannelType.Typeless, AlphaType.None)
                        }, {
                            DxgiFormat.R8G8Typeless,
                            RgbaPixelFormat.NewRg(8, 8, 0, 0, ChannelType.Typeless, AlphaType.None)
                        },
                        {DxgiFormat.R8G8Unorm, RgbaPixelFormat.NewRg(8, 8, 0, 0, ChannelType.Unorm, AlphaType.None)},
                        {DxgiFormat.R8G8Uint, RgbaPixelFormat.NewRg(8, 8, 0, 0, ChannelType.Uint, AlphaType.None)},
                        {DxgiFormat.R8G8Snorm, RgbaPixelFormat.NewRg(8, 8, 0, 0, ChannelType.Snorm, AlphaType.None)},
                        {DxgiFormat.R8G8Sint, RgbaPixelFormat.NewRg(8, 8, 0, 0, ChannelType.Sint, AlphaType.None)},
                        {DxgiFormat.R16Typeless, RgbaPixelFormat.NewR(16, 0, 0, ChannelType.Typeless, AlphaType.None)},
                        {DxgiFormat.R16Float, RgbaPixelFormat.NewR(16, 0, 0, ChannelType.Float, AlphaType.None)},
                        {DxgiFormat.R16Unorm, RgbaPixelFormat.NewR(16, 0, 0, ChannelType.Unorm, AlphaType.None)},
                        {DxgiFormat.R16Uint, RgbaPixelFormat.NewR(16, 0, 0, ChannelType.Uint, AlphaType.None)},
                        {DxgiFormat.R16Snorm, RgbaPixelFormat.NewR(16, 0, 0, ChannelType.Snorm, AlphaType.None)},
                        {DxgiFormat.R16Sint, RgbaPixelFormat.NewR(16, 0, 0, ChannelType.Sint, AlphaType.None)},
                        {DxgiFormat.R8Typeless, RgbaPixelFormat.NewR(8, 0, 0, ChannelType.Typeless, AlphaType.None)},
                        {DxgiFormat.R8Unorm, RgbaPixelFormat.NewR(8, 0, 0, ChannelType.Float, AlphaType.None)},
                        {DxgiFormat.R8Uint, RgbaPixelFormat.NewR(8, 0, 0, ChannelType.Unorm, AlphaType.None)},
                        {DxgiFormat.R8Snorm, RgbaPixelFormat.NewR(8, 0, 0, ChannelType.Uint, AlphaType.None)},
                        {DxgiFormat.R8Sint, RgbaPixelFormat.NewR(8, 0, 0, ChannelType.Sint, AlphaType.None)}, {
                            DxgiFormat.B5G6R5Unorm,
                            RgbaPixelFormat.NewBgr(5, 6, 5, 0, 0, ChannelType.Unorm, AlphaType.None)
                        },
                        {DxgiFormat.Bc1Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.None, 1)},
                        {DxgiFormat.Bc1Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.None, 1)},
                        {DxgiFormat.Bc1UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.None, 1)},
                        {DxgiFormat.Bc2Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.None, 2)},
                        {DxgiFormat.Bc2Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.None, 2)},
                        {DxgiFormat.Bc2UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.None, 2)},
                        {DxgiFormat.Bc3Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.None, 3)},
                        {DxgiFormat.Bc3Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.None, 3)},
                        {DxgiFormat.Bc3UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.None, 3)},
                        {DxgiFormat.Bc4Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.None, 4)},
                        {DxgiFormat.Bc4Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.None, 4)},
                        {DxgiFormat.Bc4Snorm, new BcPixelFormat(ChannelType.Snorm, AlphaType.None, 4)},
                        {DxgiFormat.Bc5Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.None, 5)},
                        {DxgiFormat.Bc5Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.None, 5)},
                        {DxgiFormat.Bc5Snorm, new BcPixelFormat(ChannelType.Snorm, AlphaType.None, 5)},
                        {DxgiFormat.Bc6HTypeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.None, 6)},
                        {DxgiFormat.Bc6HUf16, new BcPixelFormat(ChannelType.Uf16, AlphaType.None, 6)},
                        {DxgiFormat.Bc6HSf16, new BcPixelFormat(ChannelType.Sf16, AlphaType.None, 6)},
                        {DxgiFormat.Bc7Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.None, 7)},
                        {DxgiFormat.Bc7Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.None, 7)},
                        {DxgiFormat.Bc7UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.None, 7)},
                    }
                }, {
                    AlphaType.Straight, new Dictionary<DxgiFormat, PixelFormat> {
                        {
                            DxgiFormat.R32G32B32A32Typeless,
                            RgbaPixelFormat.NewRgba(32, 32, 32, 32, 0, 0, ChannelType.Typeless)
                        }, {
                            DxgiFormat.R32G32B32A32Float,
                            RgbaPixelFormat.NewRgba(32, 32, 32, 32, 0, 0, ChannelType.Float)
                        },
                        {DxgiFormat.R32G32B32A32Uint, RgbaPixelFormat.NewRgba(32, 32, 32, 32, 0, 0, ChannelType.Uint)},
                        {DxgiFormat.R32G32B32A32Sint, RgbaPixelFormat.NewRgba(32, 32, 32, 32, 0, 0, ChannelType.Sint)}, {
                            DxgiFormat.R16G16B16A16Typeless,
                            RgbaPixelFormat.NewRgba(16, 16, 16, 16, 0, 0, ChannelType.Typeless)
                        }, {
                            DxgiFormat.R16G16B16A16Float,
                            RgbaPixelFormat.NewRgba(16, 16, 16, 16, 0, 0, ChannelType.Float)
                        },
                        {DxgiFormat.R16G16B16A16Unorm, RgbaPixelFormat.NewRgba(16, 16, 16, 16)},
                        {DxgiFormat.R16G16B16A16Uint, RgbaPixelFormat.NewRgba(16, 16, 16, 16, 0, 0, ChannelType.Uint)}, {
                            DxgiFormat.R16G16B16A16Snorm,
                            RgbaPixelFormat.NewRgba(16, 16, 16, 16, 0, 0, ChannelType.Snorm)
                        },
                        {DxgiFormat.R16G16B16A16Sint, RgbaPixelFormat.NewRgba(16, 16, 16, 16, 0, 0, ChannelType.Sint)}, {
                            DxgiFormat.R10G10B10A2Typeless,
                            RgbaPixelFormat.NewRgba(10, 10, 10, 2, 0, 0, ChannelType.Typeless)
                        },
                        {DxgiFormat.R10G10B10A2Unorm, RgbaPixelFormat.NewRgba(10, 10, 10, 2)},
                        {DxgiFormat.R10G10B10A2Uint, RgbaPixelFormat.NewRgba(10, 10, 10, 2, 0, 0, ChannelType.Uint)},
                        {DxgiFormat.R8G8B8A8Typeless, RgbaPixelFormat.NewRgba(8, 8, 8, 8, 0, 0, ChannelType.Typeless)},
                        {DxgiFormat.R8G8B8A8Unorm, RgbaPixelFormat.NewRgba(8, 8, 8, 8)}, {
                            DxgiFormat.R8G8B8A8UnormSrgb,
                            RgbaPixelFormat.NewRgba(8, 8, 8, 8, 0, 0, ChannelType.UnormSrgb)
                        },
                        {DxgiFormat.R8G8B8A8Uint, RgbaPixelFormat.NewRgba(8, 8, 8, 8, 0, 0, ChannelType.Uint)},
                        {DxgiFormat.R8G8B8A8Snorm, RgbaPixelFormat.NewRgba(8, 8, 8, 8, 0, 0, ChannelType.Snorm)},
                        {DxgiFormat.R8G8B8A8Sint, RgbaPixelFormat.NewRgba(8, 8, 8, 8, 0, 0, ChannelType.Sint)},
                        {DxgiFormat.A8Unorm, RgbaPixelFormat.NewA(8)},
                        {DxgiFormat.B5G5R5A1Unorm, RgbaPixelFormat.NewBgra(5, 5, 5, 1)},
                        {DxgiFormat.B8G8R8A8Unorm, RgbaPixelFormat.NewBgra(8, 8, 8, 8)},
                        {DxgiFormat.B8G8R8A8Typeless, RgbaPixelFormat.NewBgra(8, 8, 8, 8, 0, 0, ChannelType.Typeless)}, {
                            DxgiFormat.B8G8R8A8UnormSrgb,
                            RgbaPixelFormat.NewBgra(8, 8, 8, 8, 0, 0, ChannelType.UnormSrgb)
                        },
                        {DxgiFormat.B4G4R4A4Unorm, RgbaPixelFormat.NewBgra(4, 4, 4, 4)},
                        {DxgiFormat.Bc1Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Straight, 1)},
                        {DxgiFormat.Bc1Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 1)},
                        {DxgiFormat.Bc1UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.Straight, 1)},
                        {DxgiFormat.Bc2Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Straight, 2)},
                        {DxgiFormat.Bc2Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 2)},
                        {DxgiFormat.Bc2UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.Straight, 2)},
                        {DxgiFormat.Bc3Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Straight, 3)},
                        {DxgiFormat.Bc3Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 3)},
                        {DxgiFormat.Bc3UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.Straight, 3)},
                        {DxgiFormat.Bc4Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Straight, 4)},
                        {DxgiFormat.Bc4Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 4)},
                        {DxgiFormat.Bc4Snorm, new BcPixelFormat(ChannelType.Snorm, AlphaType.Straight, 4)},
                        {DxgiFormat.Bc5Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Straight, 5)},
                        {DxgiFormat.Bc5Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 5)},
                        {DxgiFormat.Bc5Snorm, new BcPixelFormat(ChannelType.Snorm, AlphaType.Straight, 5)},
                        {DxgiFormat.Bc6HTypeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Straight, 6)},
                        {DxgiFormat.Bc6HUf16, new BcPixelFormat(ChannelType.Uf16, AlphaType.Straight, 6)},
                        {DxgiFormat.Bc6HSf16, new BcPixelFormat(ChannelType.Sf16, AlphaType.Straight, 6)},
                        {DxgiFormat.Bc7Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Straight, 7)},
                        {DxgiFormat.Bc7Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Straight, 7)},
                        {DxgiFormat.Bc7UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.Straight, 7)},
                    }
                }, {
                    AlphaType.Premultiplied, new Dictionary<DxgiFormat, PixelFormat> {
                        {
                            DxgiFormat.R32G32B32A32Typeless,
                            RgbaPixelFormat.NewRgba(32, 32, 32, 32, 0, 0, ChannelType.Typeless, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R32G32B32A32Float,
                            RgbaPixelFormat.NewRgba(32, 32, 32, 32, 0, 0, ChannelType.Float, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R32G32B32A32Uint,
                            RgbaPixelFormat.NewRgba(32, 32, 32, 32, 0, 0, ChannelType.Uint, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R32G32B32A32Sint,
                            RgbaPixelFormat.NewRgba(32, 32, 32, 32, 0, 0, ChannelType.Sint, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R16G16B16A16Typeless,
                            RgbaPixelFormat.NewRgba(16, 16, 16, 16, 0, 0, ChannelType.Typeless, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R16G16B16A16Float,
                            RgbaPixelFormat.NewRgba(16, 16, 16, 16, 0, 0, ChannelType.Float, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R16G16B16A16Unorm,
                            RgbaPixelFormat.NewRgba(16, 16, 16, 16, 0, 0, ChannelType.Unorm, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R16G16B16A16Uint,
                            RgbaPixelFormat.NewRgba(16, 16, 16, 16, 0, 0, ChannelType.Uint, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R16G16B16A16Snorm,
                            RgbaPixelFormat.NewRgba(16, 16, 16, 16, 0, 0, ChannelType.Snorm, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R16G16B16A16Sint,
                            RgbaPixelFormat.NewRgba(16, 16, 16, 16, 0, 0, ChannelType.Sint, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R10G10B10A2Typeless,
                            RgbaPixelFormat.NewRgba(10, 10, 10, 2, 0, 0, ChannelType.Typeless, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R10G10B10A2Unorm,
                            RgbaPixelFormat.NewRgba(10, 10, 10, 2, 0, 0, ChannelType.Unorm, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R10G10B10A2Uint,
                            RgbaPixelFormat.NewRgba(10, 10, 10, 2, 0, 0, ChannelType.Uint, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R8G8B8A8Typeless,
                            RgbaPixelFormat.NewRgba(8, 8, 8, 8, 0, 0, ChannelType.Typeless, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R8G8B8A8Unorm,
                            RgbaPixelFormat.NewRgba(8, 8, 8, 8, 0, 0, ChannelType.Unorm, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R8G8B8A8UnormSrgb,
                            RgbaPixelFormat.NewRgba(8, 8, 8, 8, 0, 0, ChannelType.UnormSrgb, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R8G8B8A8Uint,
                            RgbaPixelFormat.NewRgba(8, 8, 8, 8, 0, 0, ChannelType.Uint, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R8G8B8A8Snorm,
                            RgbaPixelFormat.NewRgba(8, 8, 8, 8, 0, 0, ChannelType.Snorm, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.R8G8B8A8Sint,
                            RgbaPixelFormat.NewRgba(8, 8, 8, 8, 0, 0, ChannelType.Sint, AlphaType.Premultiplied)
                        },
                        {DxgiFormat.A8Unorm, RgbaPixelFormat.NewA(8, 0, 0, ChannelType.Unorm, AlphaType.Premultiplied)}, {
                            DxgiFormat.B5G5R5A1Unorm,
                            RgbaPixelFormat.NewBgra(5, 5, 5, 1, 0, 0, ChannelType.Unorm, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.B8G8R8A8Unorm,
                            RgbaPixelFormat.NewBgra(8, 8, 8, 8, 0, 0, ChannelType.Unorm, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.B8G8R8A8Typeless,
                            RgbaPixelFormat.NewBgra(8, 8, 8, 8, 0, 0, ChannelType.Typeless, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.B8G8R8A8UnormSrgb,
                            RgbaPixelFormat.NewBgra(8, 8, 8, 8, 0, 0, ChannelType.UnormSrgb, AlphaType.Premultiplied)
                        }, {
                            DxgiFormat.B4G4R4A4Unorm,
                            RgbaPixelFormat.NewBgra(4, 4, 4, 4, 0, 0, ChannelType.Unorm, AlphaType.Premultiplied)
                        },
                        {DxgiFormat.Bc1Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Premultiplied, 1)},
                        {DxgiFormat.Bc1Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Premultiplied, 1)},
                        {DxgiFormat.Bc1UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.Premultiplied, 1)},
                        {DxgiFormat.Bc2Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Premultiplied, 2)},
                        {DxgiFormat.Bc2Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Premultiplied, 2)},
                        {DxgiFormat.Bc2UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.Premultiplied, 2)},
                        {DxgiFormat.Bc3Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Premultiplied, 3)},
                        {DxgiFormat.Bc3Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Premultiplied, 3)},
                        {DxgiFormat.Bc3UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.Premultiplied, 3)},
                        {DxgiFormat.Bc4Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Premultiplied, 4)},
                        {DxgiFormat.Bc4Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Premultiplied, 4)},
                        {DxgiFormat.Bc4Snorm, new BcPixelFormat(ChannelType.Snorm, AlphaType.Premultiplied, 4)},
                        {DxgiFormat.Bc5Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Premultiplied, 5)},
                        {DxgiFormat.Bc5Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Premultiplied, 5)},
                        {DxgiFormat.Bc5Snorm, new BcPixelFormat(ChannelType.Snorm, AlphaType.Premultiplied, 5)},
                        {DxgiFormat.Bc6HTypeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Premultiplied, 6)},
                        {DxgiFormat.Bc6HUf16, new BcPixelFormat(ChannelType.Uf16, AlphaType.Premultiplied, 6)},
                        {DxgiFormat.Bc6HSf16, new BcPixelFormat(ChannelType.Sf16, AlphaType.Premultiplied, 6)},
                        {DxgiFormat.Bc7Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Premultiplied, 7)},
                        {DxgiFormat.Bc7Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Premultiplied, 7)},
                        {DxgiFormat.Bc7UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.Premultiplied, 7)},
                    }
                }, {
                    AlphaType.Custom, new Dictionary<DxgiFormat, PixelFormat> {
                        {
                            DxgiFormat.B8G8R8X8Unorm,
                            RgbaPixelFormat.NewBgr(8, 8, 8, 8, 0, ChannelType.Unorm, AlphaType.Custom)
                        }, {
                            DxgiFormat.B8G8R8X8Typeless,
                            RgbaPixelFormat.NewBgr(8, 8, 8, 8, 0, ChannelType.Typeless, AlphaType.Custom)
                        }, {
                            DxgiFormat.B8G8R8X8UnormSrgb,
                            RgbaPixelFormat.NewBgr(8, 8, 8, 8, 0, ChannelType.UnormSrgb, AlphaType.Custom)
                        },
                        {DxgiFormat.Bc1Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Custom, 1)},
                        {DxgiFormat.Bc1Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Custom, 1)},
                        {DxgiFormat.Bc1UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.Custom, 1)},
                        {DxgiFormat.Bc2Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Custom, 2)},
                        {DxgiFormat.Bc2Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Custom, 2)},
                        {DxgiFormat.Bc2UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.Custom, 2)},
                        {DxgiFormat.Bc3Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Custom, 3)},
                        {DxgiFormat.Bc3Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Custom, 3)},
                        {DxgiFormat.Bc3UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.Custom, 3)},
                        {DxgiFormat.Bc4Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Custom, 4)},
                        {DxgiFormat.Bc4Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Custom, 4)},
                        {DxgiFormat.Bc4Snorm, new BcPixelFormat(ChannelType.Snorm, AlphaType.Custom, 4)},
                        {DxgiFormat.Bc5Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Custom, 5)},
                        {DxgiFormat.Bc5Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Custom, 5)},
                        {DxgiFormat.Bc5Snorm, new BcPixelFormat(ChannelType.Snorm, AlphaType.Custom, 5)},
                        {DxgiFormat.Bc6HTypeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Custom, 6)},
                        {DxgiFormat.Bc6HUf16, new BcPixelFormat(ChannelType.Uf16, AlphaType.Custom, 6)},
                        {DxgiFormat.Bc6HSf16, new BcPixelFormat(ChannelType.Sf16, AlphaType.Custom, 6)},
                        {DxgiFormat.Bc7Typeless, new BcPixelFormat(ChannelType.Typeless, AlphaType.Custom, 7)},
                        {DxgiFormat.Bc7Unorm, new BcPixelFormat(ChannelType.Unorm, AlphaType.Custom, 7)},
                        {DxgiFormat.Bc7UnormSrgb, new BcPixelFormat(ChannelType.UnormSrgb, AlphaType.Custom, 7)},
                    }
                }
            };
    }

    public static PixelFormat GetPixelFormat(DdsFourCc fourCc) =>
        FourCcToPixelFormat.TryGetValue(fourCc, out var v) ? v : UnknownPixelFormat.Instance;

    public static PixelFormat GetPixelFormat(AlphaType alphaType, DxgiFormat dxgiFormat) =>
        DxgiFormatToPixelFormat.TryGetValue(alphaType, out var d1)
            ? d1.TryGetValue(dxgiFormat, out var pf)
                ? pf
                : UnknownPixelFormat.Instance
            : UnknownPixelFormat.Instance;

    public static DdsFourCc GetFourCc(PixelFormat pf) =>
        FourCcToPixelFormat.FirstOrDefault(x => Equals(x.Value, pf)).Key;

    public static DxgiFormat GetDxgiFormat(PixelFormat pf) =>
        DxgiFormatToPixelFormat.TryGetValue(pf.Alpha, out var d1)
            ? d1.FirstOrDefault(x => Equals(x.Value, pf)).Key
            : DxgiFormat.Unknown;
}
