using System.Collections.Generic;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialSubElements;

[XmlRoot(ElementName = "SubMaterials")]
public class SubMaterials {
    [XmlElement(ElementName = "Material")]
    public List<Material> Material { get; set; } = new();
}
