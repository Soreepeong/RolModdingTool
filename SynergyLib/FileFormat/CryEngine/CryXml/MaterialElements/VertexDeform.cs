using System.Numerics;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[XmlRoot("VertexDeform")]
public class VertexDeform {
    [XmlIgnore]
    public Vector3? NoiseScale;
    
    [XmlAttribute("Type")]
    public int Type { get; set; }
    
    [XmlAttribute("DividerX")]
    public float DividerX { get; set; }
    
    [XmlAttribute("DividerY")]
    public float DividerY { get; set; }
    
    [XmlAttribute("DividerZ")]
    public float DividerZ { get; set; }
    
    [XmlAttribute("DividerW")]
    public float DividerW { get; set; }

    [XmlAttribute("NoiseScale")]
    public string? NoiseScaleString {
        get => NoiseScale.ToXmlValue();
        set => NoiseScale = value.XmlToVector3();
    }

    [XmlElement("WaveX")]
    public VertexDeformWaveAxis? WaveX { get; set; }

    [XmlElement("WaveY")]
    public VertexDeformWaveAxis? WaveY { get; set; }

    [XmlElement("WaveZ")]
    public VertexDeformWaveAxis? WaveZ { get; set; }
}