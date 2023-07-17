using System.Runtime.InteropServices;

namespace SynergyLib.FileFormat.DirectDrawSurface;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct DdsHeader {
    /// <summary>Size of structure. This member must be set to 124.</summary>
    public int Size;

    /// <summary>Flags to indicate which members contain valid data.</summary>
    public DdsHeaderFlags Flags;

    /// <summary>Surface height (in pixels).</summary>
    public int Height;

    /// <summary>Surface Width (in pixels).</summary>
    public int Width;

    /// <summary>
    /// The pitch or number of bytes per scan line in an uncompressed texture; the total number of bytes in the top
    /// level texture for a compressed texture. For information about how to compute the pitch, see the DDS File
    /// Layout section of the Programming Guide for DDS.</summary>
    public int PitchOrLinearSize;

    /// <summary>Depth of a volume texture (in pixels), otherwise unused.</summary>
    public int Depth;

    /// <summary>Number of mipmap levels, otherwise unused.</summary>
    public int MipMapCount;

    /// <summary>Unused.</summary>
    public fixed int Reserved1[11];

    /// <summary>The pixel format.</summary>
    public DdsPixelFormat PixelFormat;

    /// <summary>Specifies the complexity of the surfaces stored.</summary>
    public DdsCaps1 Caps;

    /// <summary>Additional detail about the surfaces stored.</summary>
    public DdsCaps2 Caps2;

    /// <summary>Unused.</summary>
    public int Caps3;

    /// <summary>Unused.</summary>
    public int Caps4;

    /// <summary>Unused.</summary>
    public int Reserved2;

    /// <summary>
    /// Interprets PitchOrLinearSize as Pitch value, depending on Flags.
    /// On setting this value, it will also change Flags accordingly.
    /// </summary>
    public int Pitch {
        get => Flags.HasFlag(DdsHeaderFlags.LinearSize)
            ? PitchOrLinearSize / Height
            : PitchOrLinearSize;
        set {
            Flags &= ~DdsHeaderFlags.LinearSize;
            PitchOrLinearSize = value;
        }
    }

    /// <summary>
    /// Interprets PitchOrLinearSize as LinearSize value, depending on Flags.
    /// On setting this value, it will also change Flags accordingly.
    /// </summary>
    public int LinearSize {
        get => Flags.HasFlag(DdsHeaderFlags.LinearSize)
            ? PitchOrLinearSize
            : PitchOrLinearSize * Height;
        set {
            Flags |= DdsHeaderFlags.LinearSize;
            PitchOrLinearSize = value;
        }
    }
}