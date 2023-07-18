using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

public class VertexDeformWaveAxis {
    [XmlAttribute("Type")]
    public int Type { get; set; }
    
    [XmlAttribute("Amp")]
    public float Amp { get; set; }
    
    [XmlAttribute("Level")]
    public float Level { get; set; }
    
    [XmlAttribute("Phase")]
    public float Phase { get; set; }
    
    [XmlAttribute("Freq")]
    public float Freq { get; set; }
}
