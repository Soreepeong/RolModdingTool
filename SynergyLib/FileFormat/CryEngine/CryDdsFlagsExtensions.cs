using SynergyLib.FileFormat.DirectDrawSurface;

namespace SynergyLib.FileFormat.CryEngine;

public static class CryDdsFlagsExtensions {
    public const int Magic = 0x43525946;

    public static unsafe void SetCryNonstandardHeader(this ref DdsHeader dh) {
        if (dh.Reserved2 == Magic)
            return;
        dh.Reserved2 = Magic;
        dh.Reserved1[0] = 0; // AlphaBitDepth(int:0)
        dh.Reserved1[1] = 0; // CryDdsFlags
        dh.Reserved1[2] = 0; // irrelevant
        dh.Reserved1[3] = 0; // MinColor.R(float:0)
        dh.Reserved1[4] = 0; // MinColor.G(float:0)
        dh.Reserved1[5] = 0; // MinColor.B(float:0)
        dh.Reserved1[6] = 0; // MinColor.A(float:0)
        dh.Reserved1[7] = 0x3F800000; // MaxColor.R(float:1)
        dh.Reserved1[8] = 0x3F800000; // MaxColor.G(float:1)
        dh.Reserved1[9] = 0x3F800000; // MaxColor.B(float:1)
        dh.Reserved1[10] = 0x3F800000; // MaxColor.A(float:1)
    }

    public static unsafe CryDdsFlags GetCryFlags(this in DdsHeader dh) {
        if (dh.Reserved2 != Magic)
            return 0;
        return (CryDdsFlags) dh.Reserved1[1];
    }

    public static unsafe void SetCryFlags(this ref DdsHeader dh, CryDdsFlags cdf) {
        dh.SetCryNonstandardHeader();
        dh.Reserved1[1] = (int) cdf;
    }
}
