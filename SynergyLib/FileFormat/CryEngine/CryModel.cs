using System.IO;
using System.Xml.Serialization;
using SynergyLib.FileFormat.CryEngine.CryXml;

namespace SynergyLib.FileFormat.CryEngine;

public class CryModel {
    public CryChunks Chunks;
    public MtlFile Material;

    public CryModel(string geom, string mtrl) {
        using (var fp = File.OpenRead(mtrl))
            Material = (MtlFile) new XmlSerializer(typeof(MtlFile)).Deserialize(fp)!;
        Chunks = CryChunks.FromFile(geom);
    }
}
