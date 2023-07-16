using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfAnimationChannel : BaseGltfObject {
    /// <summary>
    /// The index of a sampler in this animation used to compute the value for the target.
    /// </summary>
    /// <remarks>
    /// e.g., a node's translation, rotation, or scale (TRS).
    /// </remarks>
    [JsonProperty("sampler")]
    public int Sampler;

    /// <summary>
    /// The descriptor of the animated property.
    /// </summary>
    [JsonProperty("target")]
    public GltfAnimationChannelTarget Target = null!;
}
