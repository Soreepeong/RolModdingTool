using System;

namespace SynergyLib.FileFormat.DirectDrawSurface;

[Flags]
public enum DdsHeaderDxt10MiscFlags {
    /// <summary>Indicates a 2D texture is a cube-map texture.</summary>
    TextureCube = 4,
}