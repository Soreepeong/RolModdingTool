using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;

[XmlRoot(ElementName = "Model")]
public class Model {
    [XmlAttribute(AttributeName = "File")]
    public string? File { get; set; }

    [XmlAttribute(AttributeName = "Material")]
    public string? Material { get; set; }
}
