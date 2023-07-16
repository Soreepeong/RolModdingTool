using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfBuffer : BaseGltfObject {
    [JsonProperty("byteLength")]
    public long ByteLength;

    [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
    public string? Uri;
}
