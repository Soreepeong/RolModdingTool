using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

public class VertexDeformWaveAxis {
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("Type")]
    public int Type { get; set; }
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("Amp")]
    public float Amp { get; set; }
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("Level")]
    public float Level { get; set; }
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("Phase")]
    public float Phase { get; set; }
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("Freq")]
    public float Freq { get; set; }
}
