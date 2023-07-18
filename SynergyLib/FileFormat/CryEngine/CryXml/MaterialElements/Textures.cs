using System.Collections.Generic;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[XmlRoot("Textures")]
public record Textures {
    [XmlElement("Texture")]
    public readonly List<Texture> Texture = new();
}
