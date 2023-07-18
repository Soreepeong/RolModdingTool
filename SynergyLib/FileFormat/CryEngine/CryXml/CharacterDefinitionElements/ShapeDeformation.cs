using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;

[XmlRoot("ShapeDeformation")]
public class ShapeDeformation {
    [XmlAttribute("COL0")]
    public int Col0 { get; set; }

    [XmlAttribute("COL1")]
    public int Col1 { get; set; }

    [XmlAttribute("COL2")]
    public int Col2 { get; set; }

    [XmlAttribute("COL3")]
    public int Col3 { get; set; }

    [XmlAttribute("COL4")]
    public int Col4 { get; set; }

    [XmlAttribute("COL5")]
    public int Col5 { get; set; }

    [XmlAttribute("COL6")]
    public int Col6 { get; set; }

    [XmlAttribute("COL7")]
    public int Col7 { get; set; }
}
