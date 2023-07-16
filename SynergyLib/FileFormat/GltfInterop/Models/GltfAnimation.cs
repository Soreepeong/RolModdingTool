using System.Collections.Generic;
using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfAnimation : BaseGltfObject {
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name;

    /// <summary>
    /// An array of animation channels.
    /// </summary>
    /// <remarks>
    /// An animation channel combines an animation sampler with a target property being animated.
    /// Different channels of the same animation **MUST NOT** have the same targets.
    /// </remarks>
    [JsonProperty("channels")]
    public List<GltfAnimationChannel> Channels = new();

    /// <summary>
    /// An array of animation samplers.
    /// </summary>
    /// <remarks>
    /// An animation sampler combines timestamps with a sequence of output values and defines an interpolation
    /// algorithm.
    /// </remarks>
    [JsonProperty("samplers")]
    public List<GltfAnimationSampler> Samplers = new();
}
