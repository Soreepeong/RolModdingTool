using System.Numerics;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SynergyLib.Util.CustomJsonConverters;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[XmlRoot("VertexDeform")]
public class VertexDeform {
    [JsonConverter(typeof(Vector3JsonConverter))]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public Vector3? NoiseScale;

    [JsonIgnore]
    [XmlAttribute("NoiseScale")]
    public string? NoiseScaleString {
        get => NoiseScale.ToXmlValue();
        set => NoiseScale = value?.XmlToVector3();
    }
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("Type")]
    public int Type { get; set; }
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("DividerX")]
    public float DividerX { get; set; }
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("DividerY")]
    public float DividerY { get; set; }
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("DividerZ")]
    public float DividerZ { get; set; }
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("DividerW")]
    public float DividerW { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlElement("WaveX")]
    public VertexDeformWaveAxis? WaveX { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlElement("WaveY")]
    public VertexDeformWaveAxis? WaveY { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlElement("WaveZ")]
    public VertexDeformWaveAxis? WaveZ { get; set; }
}