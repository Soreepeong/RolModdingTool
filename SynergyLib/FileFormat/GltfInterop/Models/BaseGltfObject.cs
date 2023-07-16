using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class BaseGltfObject {
    [JsonProperty("extensions", NullValueHandling = NullValueHandling.Ignore)]
    public GltfExtensions? Extensions;
}
