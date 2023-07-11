using System.Xml.Serialization;

namespace WiiUStreamTool.FileFormat.CryEngine.CryXml.CdfSubElements;

[XmlRoot(ElementName = "Model")]
public class Model {
    [XmlAttribute(AttributeName = "File")]
    public string? File { get; set; }

    [XmlAttribute(AttributeName = "Material")]
    public string? Material { get; set; }
}