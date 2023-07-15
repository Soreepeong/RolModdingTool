using System.Collections.Generic;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[XmlRoot(ElementName = "Textures")]
public record Textures {
    [XmlElement(ElementName = "Texture")]
    public readonly List<Texture> Texture = new();
}
