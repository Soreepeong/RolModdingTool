using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfNode : BaseGltfObject {
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name;

    [JsonProperty("mesh", NullValueHandling = NullValueHandling.Ignore)]
    public int? Mesh;

    [JsonProperty("skin", NullValueHandling = NullValueHandling.Ignore)]
    public int? Skin;

    [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
    public List<int> Children = new();

    [JsonProperty("rotation", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Rotation;

    [JsonProperty("scale", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Scale;

    [JsonProperty("translation", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Translation;

    [UsedImplicitly]
    public bool ShouldSerializeChildren() => Children.Any();
}
