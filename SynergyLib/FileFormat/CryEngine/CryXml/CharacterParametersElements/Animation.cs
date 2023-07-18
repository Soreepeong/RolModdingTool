using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.CharacterParametersElements;

[XmlRoot("Animation")]
public class Animation {
    [XmlAttribute("name")]
    public string? Name { get; set; }

    [XmlAttribute("path")]
    public string? Path { get; set; }
}
