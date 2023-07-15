using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.CharacterParametersElements;

[XmlRoot(ElementName = "Comment")]
public class Comment {
    [XmlAttribute(AttributeName = "value")]
    public string? Value { get; set; }
}
