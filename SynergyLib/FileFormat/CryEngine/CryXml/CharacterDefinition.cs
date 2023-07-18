using System.Collections.Generic;
using System.Xml.Serialization;
using SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;

namespace SynergyLib.FileFormat.CryEngine.CryXml;

[XmlRoot("CharacterDefinition")]
public class CharacterDefinition {
    [XmlElement("Model")]
    public Model? Model { get; set; }

    [XmlArray("AttachmentList")]
    [XmlArrayItem("Attachment")]
    public List<Attachment>? Attachments { get; set; }

    public ShapeDeformation? ShapeDeformation { get; set; }
}
