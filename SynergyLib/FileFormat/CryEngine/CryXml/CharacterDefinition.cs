using System.Collections.Generic;
using System.Xml.Serialization;
using SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;

namespace SynergyLib.FileFormat.CryEngine.CryXml;

[XmlRoot(ElementName = "CharacterDefinition")]
public class CharacterDefinition {
    [XmlElement(ElementName = "Model")]
    public Model? Model { get; set; }

    [XmlArray(ElementName = "AttachmentList")]
    [XmlArrayItem(ElementName = "Attachment")]
    public List<Attachment>? Attachments { get; set; }

    public ShapeDeformation? ShapeDeformation { get; set; }
}
