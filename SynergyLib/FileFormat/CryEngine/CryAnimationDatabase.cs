using System.IO;

namespace SynergyLib.FileFormat.CryEngine;

public class CryAnimationDatabase {
    public CryChunks Chunks;

    public CryAnimationDatabase(Stream stream) {
        Chunks = CryChunks.FromStream(stream);
    }
}
