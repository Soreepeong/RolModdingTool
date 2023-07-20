using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfTextureInfo : BaseGltfObject {
    [JsonProperty("index", NullValueHandling = NullValueHandling.Ignore)]
    public int? Index;

    [JsonProperty("texCoord", NullValueHandling = NullValueHandling.Ignore)]
    public int? TexCoord;
}
