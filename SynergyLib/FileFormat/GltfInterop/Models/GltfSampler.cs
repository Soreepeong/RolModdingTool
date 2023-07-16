using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfSampler : BaseGltfObject {
    [JsonProperty("magFilter")]
    public GltfSamplerFilters MagFilter = GltfSamplerFilters.Linear;

    [JsonProperty("minFilter")]
    public GltfSamplerFilters MinFilter = GltfSamplerFilters.LinearMipmapLinear;
}
