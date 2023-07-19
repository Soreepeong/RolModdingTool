using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;

[XmlRoot("Parts")]
public class Parts {
    [XmlAttribute("path")]
    public string? Path { get; set; }
}
