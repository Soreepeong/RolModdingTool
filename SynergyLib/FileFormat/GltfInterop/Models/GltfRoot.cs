using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfRoot : BaseGltfObject {
    [JsonProperty("asset")]
    public GltfAsset Asset = new();

    [JsonProperty("extensionsUsed")]
    public HashSet<string> ExtensionsUsed = new();

    [JsonProperty("scene")]
    public int Scene;

    [JsonProperty("scenes")]
    public List<GltfScene> Scenes = new();

    [JsonProperty("nodes")]
    public List<GltfNode> Nodes = new();

    [JsonProperty("animations")]
    public List<GltfAnimation> Animations = new();

    [JsonProperty("materials")]
    public List<GltfMaterial> Materials = new();

    [JsonProperty("meshes")]
    public List<GltfMesh> Meshes = new();

    [JsonProperty("textures")]
    public List<GltfTexture> Textures = new();

    [JsonProperty("images")]
    public List<GltfImage> Images = new();

    [JsonProperty("skins")]
    public List<GltfSkin> Skins = new();

    [JsonProperty("accessors")]
    public List<GltfAccessor> Accessors = new();

    [JsonProperty("bufferViews")]
    public List<GltfBufferView> BufferViews = new();

    [JsonProperty("samplers")]
    public List<GltfSampler> Samplers = new();

    [JsonProperty("buffers")]
    public List<GltfBuffer> Buffers = new();

    [UsedImplicitly]
    public bool ShouldSerializeExtensionsUsed() => ExtensionsUsed.Any();

    [UsedImplicitly]
    public bool ShouldSerializeScenes() => Scenes.Any();

    [UsedImplicitly]
    public bool ShouldSerializeNodes() => Nodes.Any();

    [UsedImplicitly]
    public bool ShouldSerializeAnimations() => Animations.Any();

    [UsedImplicitly]
    public bool ShouldSerializeMaterials() => Materials.Any();

    [UsedImplicitly]
    public bool ShouldSerializeMeshes() => Meshes.Any();

    [UsedImplicitly]
    public bool ShouldSerializeTextures() => Textures.Any();

    [UsedImplicitly]
    public bool ShouldSerializeImages() => Images.Any();

    [UsedImplicitly]
    public bool ShouldSerializeSkins() => Skins.Any();

    [UsedImplicitly]
    public bool ShouldSerializeAccessors() => Accessors.Any();

    [UsedImplicitly]
    public bool ShouldSerializeBufferViews() => BufferViews.Any();

    [UsedImplicitly]
    public bool ShouldSerializeSamplers() => Samplers.Any();

    [UsedImplicitly]
    public bool ShouldSerializeBuffers() => Buffers.Any();
}
