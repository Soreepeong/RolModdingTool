using System.Collections.Generic;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[XmlRoot("SubMaterials")]
public class SubMaterials {
    [XmlElement("Material")]
    public List<Material> Material { get; set; } = new();
}
