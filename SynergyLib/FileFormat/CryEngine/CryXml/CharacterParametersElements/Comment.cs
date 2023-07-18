using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.CharacterParametersElements;

[XmlRoot("Comment")]
public class Comment {
    [XmlAttribute("value")]
    public string? Value { get; set; }
}
