using System;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Enums;

[Flags]
public enum AssetFlags : uint {
    Additive = 0x001,
    Cycle = 0x002,
    Loaded = 0x004,
    Lmg = 0x008,
    LmgValid = 0x020,
    Created = 0x800,
    Requested = 0x1000,
    Ondemand = 0x2000,
    Aimpose = 0x4000,
    AimposeUnloaded = 0x8000,
    NotFound = 0x10000,
    Tcb = 0x20000,
    Internaltype = 0x40000,
    BigEndian = 0x80000000,
}