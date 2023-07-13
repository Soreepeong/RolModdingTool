using System.Collections.Generic;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MtlSubElements;

[XmlRoot(ElementName = "Textures")]
public record Textures {
    [XmlElement(ElementName = "Texture")] public readonly List<Texture> Texture = new();
}
