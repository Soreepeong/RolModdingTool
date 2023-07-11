using System.Collections.Generic;
using System.Xml.Serialization;

namespace WiiUStreamTool.FileFormat.CryEngine.CryXml.MtlSubElements;

[XmlRoot(ElementName = "SubMaterials")]
public class SubMaterials {
    [XmlElement(ElementName = "Material")]
    public List<MtlFile> Material { get; set; } = new();
}
