using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfExtensionKhrMaterialsSpecular : BaseGltfObject {
    [JsonProperty("specularFactor", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? SpecularFactor;

    [JsonProperty("specularTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfTextureInfo? SpecularTexture;

    [JsonProperty(
        "specularColorFactor",
        DefaultValueHandling = DefaultValueHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore)]
    public float[]? SpecularColorFactor;

    [JsonProperty("specularColorTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfTextureInfo? SpecularColorTexture;
}