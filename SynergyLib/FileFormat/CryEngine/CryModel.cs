using System.IO;
using System.Xml.Serialization;
using SynergyLib.FileFormat.CryEngine.CryXml;

namespace SynergyLib.FileFormat.CryEngine;

public class CryModel {
    public CryChunks Chunks;
    public Material Material;

    public CryModel(string geom, string mtrl) {
        using (var fp = File.OpenRead(mtrl))
            Material = (Material) new XmlSerializer(typeof(Material)).Deserialize(fp)!;
        Chunks = CryChunks.FromFile(geom);
    }
}
