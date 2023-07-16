using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfExtensionMsftTextureDds : BaseGltfObject {
    [JsonProperty("source")]
    public int Source;
}
