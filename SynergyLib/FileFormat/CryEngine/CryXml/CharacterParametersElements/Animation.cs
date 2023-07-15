using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.CharacterParametersElements;

[XmlRoot(ElementName = "Animation")]
public class Animation {
    [XmlAttribute(AttributeName = "name")]
    public string? Name { get; set; }

    [XmlAttribute(AttributeName = "path")]
    public string? Path { get; set; }
}
