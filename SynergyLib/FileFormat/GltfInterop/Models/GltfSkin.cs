using System.Collections.Generic;
using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfSkin : BaseGltfObject {
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name;

    /// <summary>
    /// The index of the accessor containing the floating-point 4x4 inverse-bind matrices.
    /// </summary>
    /// <remarks>
    /// Its `accessor.count` property **MUST** be greater than or equal to the number of elements of the `joints`
    /// array. When undefined, each matrix is a 4x4 identity matrix.
    /// </remarks>
    [JsonProperty("inverseBindMatrices")]
    public int? InverseBindMatrices;

    [JsonProperty("joints")]
    public List<int>? Joints;

    [JsonProperty("extras", NullValueHandling = NullValueHandling.Ignore)]
    public GltfSkinExtras? Extras;
}

public class GltfSkinExtras : BaseGltfObject {
    public Dictionary<string, Dictionary<int, List<string>>>? Alph;
    public Dictionary<string, List<int>>? Indices;
}
