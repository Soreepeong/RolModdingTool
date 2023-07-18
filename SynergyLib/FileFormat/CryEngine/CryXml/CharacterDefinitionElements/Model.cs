using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;

[XmlRoot("Model")]
public class Model {
    [XmlAttribute("File")]
    public string? File { get; set; }

    [XmlAttribute("Material")]
    public string? Material { get; set; }
}
