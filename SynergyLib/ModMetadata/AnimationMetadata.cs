using System;
using System.ComponentModel;
using System.Numerics;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util.CustomJsonConverters;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.ModMetadata;

public class AnimationMetadata {
    private const float Epsilon = 1e-6f;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public string TargetName;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include)]
    public string? SourceName;

    [JsonProperty]
    public float MoveSpeed;

    [UsedImplicitly]
    public bool ShouldSerializeMoveSpeed() => MathF.Abs(MoveSpeed) >= Epsilon;

    [JsonProperty]
    public float TurnSpeed;

    [UsedImplicitly]
    public bool ShouldSerializeTurnSpeed() => MathF.Abs(TurnSpeed) >= Epsilon;

    [JsonProperty]
    public float AssetTurn;

    [UsedImplicitly]
    public bool ShouldSerializeAssetTurn() => MathF.Abs(AssetTurn) >= Epsilon;

    [JsonProperty]
    public float Distance;

    [UsedImplicitly]
    public bool ShouldSerializeDistance() => MathF.Abs(Distance) >= Epsilon;

    [JsonProperty]
    public float Slope;

    [UsedImplicitly]
    public bool ShouldSerializeSlope() => MathF.Abs(Slope) >= Epsilon;

    [JsonProperty]
    [JsonConverter(typeof(QuaternionJsonConverter))]
    public Quaternion StartLocationQ = Quaternion.Identity;

    [UsedImplicitly]
    public bool ShouldSerializeStartLocationQ() => !StartLocationQ.HasEquivalentValue(Quaternion.Identity, Epsilon);

    [JsonProperty]
    [JsonConverter(typeof(Vector3JsonConverter))]
    public Vector3 StartLocationV = Vector3.Zero;

    [UsedImplicitly]
    public bool ShouldSerializeStartLocationV() => !StartLocationV.HasEquivalentValue(Vector3.Zero, Epsilon);

    [JsonProperty]
    [JsonConverter(typeof(QuaternionJsonConverter))]
    public Quaternion EndLocationQ = Quaternion.Identity;

    [UsedImplicitly]
    public bool ShouldSerializeEndLocationQ() => !EndLocationQ.HasEquivalentValue(Quaternion.Identity, Epsilon);

    [JsonProperty]
    [JsonConverter(typeof(Vector3JsonConverter))]
    public Vector3 EndLocationV = Vector3.Zero;

    [UsedImplicitly]
    public bool ShouldSerializeEndLocationV() => !EndLocationV.HasEquivalentValue(Vector3.Zero, Epsilon);

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(-10000f)]
    public float LHeelStart = -10000f;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(-10000f)]
    public float LHeelEnd = -10000f;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(-10000f)]
    public float LToe0Start = -10000f;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(-10000f)]
    public float LToe0End = -10000f;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(-10000f)]
    public float RHeelStart = -10000f;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(-10000f)]
    public float RHeelEnd = -10000f;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(-10000f)]
    public float RToe0Start = -10000f;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(-10000f)]
    public float RToe0End = -10000f;

    public AnimationMetadata() : this(string.Empty) { }

    public AnimationMetadata(string targetName) {
        TargetName = targetName;
    }

    public AnimationMetadata(string targetName, in ControllerMotionParams mp) {
        TargetName = targetName;
        CopyFromMotionParams(mp);
    }

    public void CopyFromMotionParams(in ControllerMotionParams mp) {
        MoveSpeed = mp.MoveSpeed;
        TurnSpeed = mp.TurnSpeed;
        AssetTurn = mp.AssetTurn;
        Distance = mp.Distance;
        Slope = mp.Slope;
        StartLocationQ = mp.StartLocationQ;
        StartLocationV = mp.StartLocationV;
        EndLocationQ = mp.EndLocationQ;
        EndLocationV = mp.EndLocationV;
        LHeelStart = mp.LHeelStart;
        LHeelEnd = mp.LHeelEnd;
        LToe0Start = mp.LToe0Start;
        LToe0End = mp.LToe0End;
        RHeelStart = mp.RHeelStart;
        RHeelEnd = mp.RHeelEnd;
        RToe0Start = mp.RToe0Start;
        RToe0End = mp.RToe0End;
    }
}
