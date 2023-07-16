using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfAsset : BaseGltfObject {
    [JsonProperty("generator", NullValueHandling = NullValueHandling.Ignore)]
    public string? Generator;

    [JsonProperty("version")]
    public string Version = "2.0";
}
