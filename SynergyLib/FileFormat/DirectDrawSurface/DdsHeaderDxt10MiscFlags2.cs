using System;

namespace SynergyLib.FileFormat.DirectDrawSurface;

[Flags]
public enum DdsHeaderDxt10MiscFlags2 {
    /// <summary>
    /// Alpha channel content is unknown. This is the value for legacy files, which typically is assumed to be
    /// 'straight' alpha.
    /// </summary>
    AlphaModeUnknown = 0x0,
    
    /// <summary>Any alpha channel content is presumed to use straight alpha.</summary>
    AlphaModeStraight = 0x1,
    
    /// <summary>
    /// Any alpha channel content is using premultiplied alpha. The only legacy file formats that indicate this
    /// information are 'DX2' and 'DX4'.	
    /// </summary>
    AlphaModePremultiplied = 0x2,
    
    /// <summary>Any alpha channel content is all set to fully opaque.</summary>
    AlphaModeOpaque = 0x3,
    
    /// <summary>
    /// Any alpha channel content is being used as a 4th channel and is not intended to represent transparency
    /// (straight or premultiplied).	
    /// </summary>
    AlphaModeCustom = 0x4,
}