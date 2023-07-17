using System.Runtime.InteropServices;

namespace SynergyLib.FileFormat.DirectDrawSurface;

[StructLayout(LayoutKind.Sequential)]
public struct DdsHeaderDxt10 {
    /// <summary>The surface pixel format.</summary>
    public DxgiFormat DxgiFormat;

    /// <summary>Identifies the type of resource.</summary>
    public DdsHeaderDxt10ResourceDimension ResourceDimension;

    /// <summary>Identifies other, less common options for resources.</summary>
    public DdsHeaderDxt10MiscFlags MiscFlag;

    /// <summary>
    /// The number of elements in the array.
    /// 
    /// For a 2D texture that is also a cube-map texture, this number represents the number of cubes. This number is
    /// the same as the number in the NumCubes member of D3D10_TEXCUBE_ARRAY_SRV1 or D3D11_TEXCUBE_ARRAY_SRV). In this
    /// case, the DDS file contains arraySize*6 2D textures. For more information about this case, see the miscFlag
    /// description.
    /// 
    /// For a 3D texture, you must set this number to 1.
    /// </summary>
    public int ArraySize;

    /// <summary>
    /// Contains additional metadata (formerly was reserved). The lower 3 bits indicate the alpha mode of the
    /// associated resource. The upper 29 bits are reserved and are typically 0.
    /// </summary>
    public DdsHeaderDxt10MiscFlags2 MiscFlags2;
}