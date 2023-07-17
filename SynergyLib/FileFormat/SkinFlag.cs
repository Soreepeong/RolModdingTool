using System.IO;

namespace SynergyLib.FileFormat;

public enum SkinFlag : short {
    Default = 0,
    Sonic = -1,
    Tails = -2,
    Amy = -3,
    Knuckles = -4,
    SonicAlt = 1,
    TailsAlt = 2,
    AmyAlt = 3,
    KnucklesAlt = 4,
    
    LookupDefault = 0x7FFF,
    LookupAlt = 0x7FFE,
}

public static class SkinFlagExtensions {
    public static string TransformPath(this SkinFlag flag, string path) {
        if (flag > SkinFlag.Default) {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext is ".dds" or ".mtl") {
                var dirName = Path.GetDirectoryName(path);
                path = Path.GetFileNameWithoutExtension(path) + ".alt" + ext;
                if (dirName is not null)
                    path = Path.Combine(dirName, path);
            }
        }

        path = path.Replace('/', '\\');
        return path;
    }

    public static bool IsAltSkin(this SkinFlag flag) =>
        flag is SkinFlag.SonicAlt or SkinFlag.TailsAlt or SkinFlag.AmyAlt or SkinFlag.KnucklesAlt;

    public static bool MatchesLookup(this SkinFlag flag, SkinFlag lookupFlag) {
        if (flag == lookupFlag)
            return true;
        
        switch (lookupFlag) {
            case SkinFlag.LookupDefault when (flag == SkinFlag.Default || !flag.IsAltSkin()):
            case SkinFlag.LookupAlt when (flag == SkinFlag.Default || flag.IsAltSkin()):
                return true;
            default:
                return false;
        }
    }
}
