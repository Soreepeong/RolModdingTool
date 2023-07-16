using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfMaterialPbrMetallicRoughness : BaseGltfObject {
    [JsonProperty(
        "baseColorFactor",
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float[]? BaseColorFactor = {1f, 1f, 1f, 1f};

    [JsonProperty("baseColorTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfTextureInfo? BaseColorTexture;

    [JsonProperty(
        "metallicFactor",
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? MetallicFactor = 1f;

    [JsonProperty(
        "roughnessFactor",
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? RoughnessFactor = 1f;

    [JsonProperty("metallicRoughnessTexture", NullValueHandling = NullValueHandling.Ignore)]
    public GltfTextureInfo? MetallicRoughnessTexture;
}