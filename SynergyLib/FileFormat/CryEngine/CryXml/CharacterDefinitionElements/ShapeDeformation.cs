using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;

[XmlRoot(ElementName = "ShapeDeformation")]
public class ShapeDeformation {
    [XmlAttribute(AttributeName = "COL0")]
    public int Col0 { get; set; }

    [XmlAttribute(AttributeName = "COL1")]
    public int Col1 { get; set; }

    [XmlAttribute(AttributeName = "COL2")]
    public int Col2 { get; set; }

    [XmlAttribute(AttributeName = "COL3")]
    public int Col3 { get; set; }

    [XmlAttribute(AttributeName = "COL4")]
    public int Col4 { get; set; }

    [XmlAttribute(AttributeName = "COL5")]
    public int Col5 { get; set; }

    [XmlAttribute(AttributeName = "COL6")]
    public int Col6 { get; set; }

    [XmlAttribute(AttributeName = "COL7")]
    public int Col7 { get; set; }
}
