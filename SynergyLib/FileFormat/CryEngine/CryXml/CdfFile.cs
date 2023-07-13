using System;
using System.Xml.Serialization;
using SynergyLib.FileFormat.CryEngine.CryXml.CdfSubElements;

namespace SynergyLib.FileFormat.CryEngine.CryXml;

[XmlRoot(ElementName = "CharacterDefinition")]
public class CdfFile {
    [XmlElement(ElementName = "Model")]
    public Model Model { get; set; } = new();

    [XmlArray(ElementName = "AttachmentList")]
    [XmlArrayItem(ElementName = "Attachment")]
    public Attachment[] Attachments { get; set; } = Array.Empty<Attachment>();

    public ShapeDeformation ShapeDeformation { get; set; } = new();

}
