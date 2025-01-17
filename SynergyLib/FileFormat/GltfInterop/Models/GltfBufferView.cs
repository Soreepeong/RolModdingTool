﻿using Newtonsoft.Json;

namespace SynergyLib.FileFormat.GltfInterop.Models;

public class GltfBufferView : BaseGltfObject {
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name;

    [JsonProperty("buffer")]
    public int Buffer;

    [JsonProperty("byteLength")]
    public long ByteLength;

    [JsonProperty("byteOffset")]
    public long ByteOffset;

    [JsonProperty("target", NullValueHandling = NullValueHandling.Ignore)]
    public GltfBufferViewTarget? Target;

    [JsonIgnore]
    public long ByteOffsetTo => ByteLength + ByteOffset;
}
