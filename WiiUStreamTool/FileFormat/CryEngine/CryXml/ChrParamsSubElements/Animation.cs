using System.Xml.Serialization;

namespace WiiUStreamTool.FileFormat.CryEngine.CryXml.ChrParamsSubElements;

[XmlRoot(ElementName = "Animation")]
public class Animation {
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "path")]
    public string Path { get; set; } = string.Empty;
}
