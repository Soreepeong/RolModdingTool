namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;

public enum VectorCompressionFormat {
    NoCompress = 0,
    NoCompressQuat = 1,
    NoCompressVec3 = 2,
    ShortInt3Quat = 3,
    SmallTreeQuat32 = 4,
    SmallTreeQuat48 = 5,
    SmallTreeQuat64 = 6,
    PolarQuat = 7,
    SmallTreeQuat64Ext = 8,
}
