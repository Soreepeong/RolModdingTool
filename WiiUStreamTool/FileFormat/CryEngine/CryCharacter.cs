using System.IO;
using System.Xml.Serialization;
using WiiUStreamTool.FileFormat.CryEngine.CryXml;

namespace WiiUStreamTool.FileFormat.CryEngine;

public class CryCharacter {
    public CdfFile Definition;
    public CryModel Model;

    public CryCharacter(string basePath, string cdfPath) {
        using (var fp = File.OpenRead(Path.Join(basePath, cdfPath)))
            Definition = (CdfFile) new XmlSerializer(typeof(CdfFile)).Deserialize(fp)!;
        Model = new(Path.Join(basePath, Definition.Model.File), Path.Join(basePath, Definition.Model.Material));
    }
}
