using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.ChrParamsSubElements;

[XmlRoot(ElementName = "Comment")]
public class Comment {
    [XmlAttribute(AttributeName = "path")]
    public string Value { get; set; } = string.Empty;
}
