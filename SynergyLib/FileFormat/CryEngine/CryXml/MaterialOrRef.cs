using System.Xml.Serialization;
using JsonSubTypes;
using Newtonsoft.Json;

namespace SynergyLib.FileFormat.CryEngine.CryXml;

[JsonConverter(typeof(JsonSubtypes), "IsReference")]
[JsonSubtypes.KnownSubType(typeof(Material), true)]
[JsonSubtypes.KnownSubType(typeof(MaterialRef), false)]
public class MaterialOrRef {
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlIgnore]
    public bool IsReference;
}