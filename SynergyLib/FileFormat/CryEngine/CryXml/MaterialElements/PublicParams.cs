using System;
using System.Numerics;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SynergyLib.Util.CustomJsonConverters;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[XmlRoot("PublicParams")]
public class PublicParams : ICloneable {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(Vector3JsonConverter))]
    [XmlIgnore]
    public Vector3? SilhouetteColor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(Vector3JsonConverter))]
    [XmlIgnore]
    public Vector3? IrisColor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(Vector3JsonConverter))]
    [XmlIgnore]
    public Vector3? IndirectColor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(Vector3JsonConverter))]
    [XmlIgnore]
    public Vector3? WrapColor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(Vector3JsonConverter))]
    [XmlIgnore]
    public Vector3? BackDiffuse;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(Vector3JsonConverter))]
    [XmlIgnore]
    public Vector3? TintColor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(Vector3JsonConverter))]
    [XmlIgnore]
    public Vector3? LiquidColor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(Vector3JsonConverter))]
    [XmlIgnore]
    public Vector3? FogColor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? Visibility;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? IrisAspectRatio;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? IrisRotate;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? IrisSize;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? EyeSadFactor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? PupilScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? SilhouetteIntensity;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FresnelPower;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FresnelScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FresnelBias;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? IrisParallaxOffset;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? EyeRotateVertical;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? EyeRotateHorizontal;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? EyeRotationMultiplier;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? Metalness;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? WrapDiffuse;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BendDetailLeafAmplitude;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BendDetailFrequency;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BendDetailBranchAmplitude;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendFalloff;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendLayer2Tiling;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendFactor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendWithTerrainAmount;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendMaskRotation;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendNormalAddFactor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BackDiffuseMultiplier;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BackShadowBias;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BackViewDep;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? CapOpacityFalloff;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BackLightScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BumpScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? IndexOfRefraction;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? TintCloudiness;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? GlossFromDiffuseContrast;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? GlossFromDiffuseOffset;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? GlossFromDiffuseAmount;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? GlossFromDiffuseBrightness;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public int? SpecMapChannelR;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public int? SpecMapChannelG;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public int? SpecMapChannelB;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public int? GlossMapChannelR;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public int? GlossMapChannelG;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public int? GlossMapChannelB;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FlowTilling;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? GlowRimFactor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? ShieldOn;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? ShieldHit;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FlowSpeed;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FogDensity;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? HdrDynamic;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? AmbientMultiplier;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? SoftIntersectionFactor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? SunMultiplier;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? TillingLayer0;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? SpeedLayer0;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? SpeedLayer1;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? TillingLayer1;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FoamMultiplier;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? RedShift;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? RefractionBumpScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? ReflectionAmount;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? ReflectAmount;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BumpScaleLayer1;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FoamDeform;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FoamAmount;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BumpScaleLayer0;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? AnimAmplitudeWav0;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FuzzynessSpread;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FuzzynessStrength;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FuzzynessSmoothness;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FuzzynessSaturation;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? DiffuseExponent;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? AnimFrequency;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? AnimAmplitudeWav2;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? AnimPhase;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? Noise;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? NoiseScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? ScaleXz;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? ScaleXzEnd;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? NoiseSpeed;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? SpecFromDiffAlphaBias;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? SpecFromDiffAlphaMult;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FogCutoffEnd;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BungeeWobbleLoose;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BungeeWobbleMult;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BungeeTension;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BungeeWobbleTense;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public int? BungeeMode;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BungeeWobble;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? GlowCausticFade;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? AnisotropicShape;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? StartRadius;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? EndRadius;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(Vector3JsonConverter))]
    [XmlIgnore]
    public Vector3? StartColor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(Vector3JsonConverter))]
    [XmlIgnore]
    public Vector3? EndColor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FinalMultiplier;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BeamLength;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BeamOriginFade;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? ViewDependencyFactor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? OrigLength;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? OrigWidth;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public int? BlendLayerToDefault;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? ParallaxOffset;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlurAmount;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? SelfShadowStrength;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? ObmDisplacement;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? HeightBias;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? DetailBumpTillingU;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? DetailDiffuseScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? DetailBumpScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? DetailGlossScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? NormalsScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? Tilling;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FoamCrestAmount;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? SubSurfaceScatteringScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? RipplesNormalsScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? DetailNormalsScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? DetailTilling;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? ReflectionScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FoamSoftIntersectionFactor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FoamTilling;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? ReflectionBumpScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? RainTilling;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? PomDisplacement;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public int? ParallaxShadowMode;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendLayerAlphaConstant;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? SpecularNoiseTiling;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public int? SpecularNoiseMode;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? CloudDistantTiling;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? CloudSkyColorMultiplier;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FogOpacity;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? CloudDistantHeight;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FogBottomToTopColor;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? FogHeight;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public int? BlurEnvironmentMap;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? CloudDistantScrollSpeedX;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? CloudOpacity;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? CloudDistantScrollSpeedY;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? CubeRotateZ;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? CubeRotationSpeed;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? CloudTranslucency;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? CubemapStretch;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? WaterDistantBump;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? WaterReflectionStrength;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? CubemapHorizonOffset;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? GrainAmount;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? DeformAmount;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? DeformTillingY;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? GrainTilling;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? InterlacingTilling;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? DeformTillingX;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? GrainSaturation;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? InterlacingAmount;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? VSyncFreq;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? ChromaShift;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? DeformFreq;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendVectorBump;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendVectorCoverage;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendVectorFeather;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendVectorWorldX;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendVectorWorldY;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendVectorWorldZ;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendHeightScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? WaterWallBumpScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? WaterWallFoamAmount;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? WaterWallParallaxOffset;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? WaterWallRefractionBumpScale;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? WaterWallSunshaftIntensity;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public float? BlendMaskTiling;
    
    public object Clone() => MemberwiseClone();
    
    [JsonIgnore]
    [XmlAttribute("SilhouetteColor")]
    public string? SilhouetteColorString {
        get => SilhouetteColor.ToXmlValue();
        set => SilhouetteColor = value?.XmlToVector3();
    }

    [JsonIgnore]
    [XmlAttribute("IrisColor")]
    public string? IrisColorString {
        get => IrisColor.ToXmlValue();
        set => IrisColor = value?.XmlToVector3();
    }

    [JsonIgnore]
    [XmlAttribute("IndirectColor")]
    public string? IndirectColorString {
        get => IndirectColor.ToXmlValue();
        set => IndirectColor = value?.XmlToVector3();
    }

    [JsonIgnore]
    [XmlAttribute("WrapColor")]
    public string? WrapColorString {
        get => WrapColor.ToXmlValue();
        set => WrapColor = value?.XmlToVector3();
    }

    [JsonIgnore]
    [XmlAttribute("BackDiffuse")]
    public string? BackDiffuseString {
        get => BackDiffuse.ToXmlValue();
        set => BackDiffuse = value?.XmlToVector3();
    }

    [JsonIgnore]
    [XmlAttribute("TintColor")]
    public string? TintColorString {
        get => TintColor.ToXmlValue();
        set => TintColor = value?.XmlToVector3();
    }

    [JsonIgnore]
    [XmlAttribute("LiquidColor")]
    public string? LiquidColorString {
        get => LiquidColor.ToXmlValue();
        set => LiquidColor = value?.XmlToVector3();
    }

    [JsonIgnore]
    [XmlAttribute("FogColor")]
    public string? FogColorString {
        get => FogColor.ToXmlValue();
        set => FogColor = value?.XmlToVector3();
    }

    [JsonIgnore]
    [XmlAttribute("Visibility")]
    public string? VisibilityString {
        get => Visibility?.ToString();
        set => Visibility = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("IrisAspectRatio")]
    public string? IrisAspectRatioString {
        get => IrisAspectRatio?.ToString();
        set => IrisAspectRatio = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("IrisRotate")]
    public string? IrisRotateString {
        get => IrisRotate?.ToString();
        set => IrisRotate = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("IrisSize")]
    public string? IrisSizeString {
        get => IrisSize?.ToString();
        set => IrisSize = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("EyeSadFactor")]
    public string? EyeSadFactorString {
        get => EyeSadFactor?.ToString();
        set => EyeSadFactor = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("PupilScale")]
    public string? PupilScaleString {
        get => PupilScale?.ToString();
        set => PupilScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SilhouetteIntensity")]
    public string? SilhouetteIntensityString {
        get => SilhouetteIntensity?.ToString();
        set => SilhouetteIntensity = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FresnelPower")]
    public string? FresnelPowerString {
        get => FresnelPower?.ToString();
        set => FresnelPower = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FresnelScale")]
    public string? FresnelScaleString {
        get => FresnelScale?.ToString();
        set => FresnelScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FresnelBias")]
    public string? FresnelBiasString {
        get => FresnelBias?.ToString();
        set => FresnelBias = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("IrisParallaxOffset")]
    public string? IrisParallaxOffsetString {
        get => IrisParallaxOffset?.ToString();
        set => IrisParallaxOffset = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("EyeRotateVertical")]
    public string? EyeRotateVerticalString {
        get => EyeRotateVertical?.ToString();
        set => EyeRotateVertical = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("EyeRotateHorizontal")]
    public string? EyeRotateHorizontalString {
        get => EyeRotateHorizontal?.ToString();
        set => EyeRotateHorizontal = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("EyeRotationMultiplier")]
    public string? EyeRotationMultiplierString {
        get => EyeRotationMultiplier?.ToString();
        set => EyeRotationMultiplier = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("Metalness")]
    public string? MetalnessString {
        get => Metalness?.ToString();
        set => Metalness = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("WrapDiffuse")]
    public string? WrapDiffuseString {
        get => WrapDiffuse?.ToString();
        set => WrapDiffuse = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("bendDetailLeafAmplitude")]
    public string? BendDetailLeafAmplitudeString {
        get => BendDetailLeafAmplitude?.ToString();
        set => BendDetailLeafAmplitude = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("bendDetailFrequency")]
    public string? BendDetailFrequencyString {
        get => BendDetailFrequency?.ToString();
        set => BendDetailFrequency = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("bendDetailBranchAmplitude")]
    public string? BendDetailBranchAmplitudeString {
        get => BendDetailBranchAmplitude?.ToString();
        set => BendDetailBranchAmplitude = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendFalloff")]
    public string? BlendFalloffString {
        get => BlendFalloff?.ToString();
        set => BlendFalloff = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendLayer2Tiling")]
    public string? BlendLayer2TilingString {
        get => BlendLayer2Tiling?.ToString();
        set => BlendLayer2Tiling = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendFactor")]
    public string? BlendFactorString {
        get => BlendFactor?.ToString();
        set => BlendFactor = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("blendWithTerrainAmount")]
    public string? BlendWithTerrainAmountString {
        get => BlendWithTerrainAmount?.ToString();
        set => BlendWithTerrainAmount = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendMaskRotation")]
    public string? BlendMaskRotationString {
        get => BlendMaskRotation?.ToString();
        set => BlendMaskRotation = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendNormalAddFactor")]
    public string? BlendNormalAddFactorString {
        get => BlendNormalAddFactor?.ToString();
        set => BlendNormalAddFactor = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BackDiffuseMultiplier")]
    public string? BackDiffuseMultiplierString {
        get => BackDiffuseMultiplier?.ToString();
        set => BackDiffuseMultiplier = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BackShadowBias")]
    public string? BackShadowBiasString {
        get => BackShadowBias?.ToString();
        set => BackShadowBias = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BackViewDep")]
    public string? BackViewDepString {
        get => BackViewDep?.ToString();
        set => BackViewDep = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("CapOpacityFalloff")]
    public string? CapOpacityFalloffString {
        get => CapOpacityFalloff?.ToString();
        set => CapOpacityFalloff = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BackLightScale")]
    public string? BackLightScaleString {
        get => BackLightScale?.ToString();
        set => BackLightScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BumpScale")]
    public string? BumpScaleString {
        get => BumpScale?.ToString();
        set => BumpScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("IndexOfRefraction")]
    public string? IndexOfRefractionString {
        get => IndexOfRefraction?.ToString();
        set => IndexOfRefraction = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("TintCloudiness")]
    public string? TintCloudinessString {
        get => TintCloudiness?.ToString();
        set => TintCloudiness = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("GlossFromDiffuseContrast")]
    public string? GlossFromDiffuseContrastString {
        get => GlossFromDiffuseContrast?.ToString();
        set => GlossFromDiffuseContrast = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("GlossFromDiffuseOffset")]
    public string? GlossFromDiffuseOffsetString {
        get => GlossFromDiffuseOffset?.ToString();
        set => GlossFromDiffuseOffset = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("GlossFromDiffuseAmount")]
    public string? GlossFromDiffuseAmountString {
        get => GlossFromDiffuseAmount?.ToString();
        set => GlossFromDiffuseAmount = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("GlossFromDiffuseBrightness")]
    public string? GlossFromDiffuseBrightnessString {
        get => GlossFromDiffuseBrightness?.ToString();
        set => GlossFromDiffuseBrightness = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SpecMapChannelR")]
    public string? SpecMapChannelRString {
        get => SpecMapChannelR?.ToString();
        set => SpecMapChannelR = value is null ? null : int.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SpecMapChannelG")]
    public string? SpecMapChannelGString {
        get => SpecMapChannelG?.ToString();
        set => SpecMapChannelG = value is null ? null : int.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SpecMapChannelB")]
    public string? SpecMapChannelBString {
        get => SpecMapChannelB?.ToString();
        set => SpecMapChannelB = value is null ? null : int.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("GlossMapChannelR")]
    public string? GlossMapChannelRString {
        get => GlossMapChannelR?.ToString();
        set => GlossMapChannelR = value is null ? null : int.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("GlossMapChannelG")]
    public string? GlossMapChannelGString {
        get => GlossMapChannelG?.ToString();
        set => GlossMapChannelG = value is null ? null : int.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("GlossMapChannelB")]
    public string? GlossMapChannelBString {
        get => GlossMapChannelB?.ToString();
        set => GlossMapChannelB = value is null ? null : int.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FlowTilling")]
    public string? FlowTillingString {
        get => FlowTilling?.ToString();
        set => FlowTilling = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("GlowRimFactor")]
    public string? GlowRimFactorString {
        get => GlowRimFactor?.ToString();
        set => GlowRimFactor = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("ShieldOn")]
    public string? ShieldOnString {
        get => ShieldOn?.ToString();
        set => ShieldOn = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("ShieldHit")]
    public string? ShieldHitString {
        get => ShieldHit?.ToString();
        set => ShieldHit = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FlowSpeed")]
    public string? FlowSpeedString {
        get => FlowSpeed?.ToString();
        set => FlowSpeed = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FogDensity")]
    public string? FogDensityString {
        get => FogDensity?.ToString();
        set => FogDensity = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("HDRDynamic")]
    public string? HdrDynamicString {
        get => HdrDynamic?.ToString();
        set => HdrDynamic = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("AmbientMultiplier")]
    public string? AmbientMultiplierString {
        get => AmbientMultiplier?.ToString();
        set => AmbientMultiplier = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SoftIntersectionFactor")]
    public string? SoftIntersectionFactorString {
        get => SoftIntersectionFactor?.ToString();
        set => SoftIntersectionFactor = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SunMultiplier")]
    public string? SunMultiplierString {
        get => SunMultiplier?.ToString();
        set => SunMultiplier = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("TillingLayer0")]
    public string? TillingLayer0String {
        get => TillingLayer0?.ToString();
        set => TillingLayer0 = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SpeedLayer0")]
    public string? SpeedLayer0String {
        get => SpeedLayer0?.ToString();
        set => SpeedLayer0 = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SpeedLayer1")]
    public string? SpeedLayer1String {
        get => SpeedLayer1?.ToString();
        set => SpeedLayer1 = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("TillingLayer1")]
    public string? TillingLayer1String {
        get => TillingLayer1?.ToString();
        set => TillingLayer1 = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FoamMultiplier")]
    public string? FoamMultiplierString {
        get => FoamMultiplier?.ToString();
        set => FoamMultiplier = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("RedShift")]
    public string? RedShiftString {
        get => RedShift?.ToString();
        set => RedShift = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("RefractionBumpScale")]
    public string? RefractionBumpScaleString {
        get => RefractionBumpScale?.ToString();
        set => RefractionBumpScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("ReflectionAmount")]
    public string? ReflectionAmountString {
        get => ReflectionAmount?.ToString();
        set => ReflectionAmount = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("ReflectAmount")]
    public string? ReflectAmountString {
        get => ReflectAmount?.ToString();
        set => ReflectAmount = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BumpScaleLayer1")]
    public string? BumpScaleLayer1String {
        get => BumpScaleLayer1?.ToString();
        set => BumpScaleLayer1 = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FoamDeform")]
    public string? FoamDeformString {
        get => FoamDeform?.ToString();
        set => FoamDeform = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FoamAmount")]
    public string? FoamAmountString {
        get => FoamAmount?.ToString();
        set => FoamAmount = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BumpScaleLayer0")]
    public string? BumpScaleLayer0String {
        get => BumpScaleLayer0?.ToString();
        set => BumpScaleLayer0 = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("AnimAmplitudeWav0")]
    public string? AnimAmplitudeWav0String {
        get => AnimAmplitudeWav0?.ToString();
        set => AnimAmplitudeWav0 = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FuzzynessSpread")]
    public string? FuzzynessSpreadString {
        get => FuzzynessSpread?.ToString();
        set => FuzzynessSpread = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FuzzynessStrength")]
    public string? FuzzynessStrengthString {
        get => FuzzynessStrength?.ToString();
        set => FuzzynessStrength = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FuzzynessSmoothness")]
    public string? FuzzynessSmoothnessString {
        get => FuzzynessSmoothness?.ToString();
        set => FuzzynessSmoothness = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FuzzynessSaturation")]
    public string? FuzzynessSaturationString {
        get => FuzzynessSaturation?.ToString();
        set => FuzzynessSaturation = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("DiffuseExponent")]
    public string? DiffuseExponentString {
        get => DiffuseExponent?.ToString();
        set => DiffuseExponent = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("AnimFrequency")]
    public string? AnimFrequencyString {
        get => AnimFrequency?.ToString();
        set => AnimFrequency = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("AnimAmplitudeWav2")]
    public string? AnimAmplitudeWav2String {
        get => AnimAmplitudeWav2?.ToString();
        set => AnimAmplitudeWav2 = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("AnimPhase")]
    public string? AnimPhaseString {
        get => AnimPhase?.ToString();
        set => AnimPhase = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("Noise")]
    public string? NoiseString {
        get => Noise?.ToString();
        set => Noise = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("NoiseScale")]
    public string? NoiseScaleString {
        get => NoiseScale?.ToString();
        set => NoiseScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("ScaleXZ")]
    public string? ScaleXzString {
        get => ScaleXz?.ToString();
        set => ScaleXz = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("ScaleXZend")]
    public string? ScaleXzEndString {
        get => ScaleXzEnd?.ToString();
        set => ScaleXzEnd = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("NoiseSpeed")]
    public string? NoiseSpeedString {
        get => NoiseSpeed?.ToString();
        set => NoiseSpeed = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SpecFromDiffAlphaBias")]
    public string? SpecFromDiffAlphaBiasString {
        get => SpecFromDiffAlphaBias?.ToString();
        set => SpecFromDiffAlphaBias = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SpecFromDiffAlphaMult")]
    public string? SpecFromDiffAlphaMultString {
        get => SpecFromDiffAlphaMult?.ToString();
        set => SpecFromDiffAlphaMult = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FogCutoffEnd")]
    public string? FogCutoffEndString {
        get => FogCutoffEnd?.ToString();
        set => FogCutoffEnd = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BungeeWobbleLoose")]
    public string? BungeeWobbleLooseString {
        get => BungeeWobbleLoose?.ToString();
        set => BungeeWobbleLoose = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BungeeWobbleMult")]
    public string? BungeeWobbleMultString {
        get => BungeeWobbleMult?.ToString();
        set => BungeeWobbleMult = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BungeeTension")]
    public string? BungeeTensionString {
        get => BungeeTension?.ToString();
        set => BungeeTension = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BungeeWobbleTense")]
    public string? BungeeWobbleTenseString {
        get => BungeeWobbleTense?.ToString();
        set => BungeeWobbleTense = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BungeeMode")]
    public string? BungeeModeString {
        get => BungeeMode?.ToString();
        set => BungeeMode = value is null ? null : int.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BungeeWobble")]
    public string? BungeeWobbleString {
        get => BungeeWobble?.ToString();
        set => BungeeWobble = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("GlowCausticFade")]
    public string? GlowCausticFadeString {
        get => GlowCausticFade?.ToString();
        set => GlowCausticFade = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("AnisotropicShape")]
    public string? AnisotropicShapeString {
        get => AnisotropicShape?.ToString();
        set => AnisotropicShape = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("StartRadius")]
    public string? StartRadiusString {
        get => StartRadius?.ToString();
        set => StartRadius = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("EndRadius")]
    public string? EndRadiusString {
        get => EndRadius?.ToString();
        set => EndRadius = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("StartColor")]
    public string? StartColorString {
        get => StartColor?.ToXmlValue();
        set => StartColor = value?.XmlToVector3();
    }

    [JsonIgnore]
    [XmlAttribute("EndColor")]
    public string? EndColorString {
        get => EndColor?.ToString();
        set => EndColor = value?.XmlToVector3();
    }

    [JsonIgnore]
    [XmlAttribute("FinalMultiplier")]
    public string? FinalMultiplierString {
        get => FinalMultiplier?.ToString();
        set => FinalMultiplier = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BeamLength")]
    public string? BeamLengthString {
        get => BeamLength?.ToString();
        set => BeamLength = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BeamOriginFade")]
    public string? BeamOriginFadeString {
        get => BeamOriginFade?.ToString();
        set => BeamOriginFade = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("viewDependencyFactor")]
    public string? ViewDependencyFactorString {
        get => ViewDependencyFactor?.ToString();
        set => ViewDependencyFactor = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("OrigLength")]
    public string? OrigLengthString {
        get => OrigLength?.ToString();
        set => OrigLength = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("OrigWidth")]
    public string? OrigWidthString {
        get => OrigWidth?.ToString();
        set => OrigWidth = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendLayerToDefault")]
    public string? BlendLayerToDefaultString {
        get => BlendLayerToDefault?.ToString();
        set => BlendLayerToDefault = value is null ? null : int.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("ParallaxOffset")]
    public string? ParallaxOffsetString {
        get => ParallaxOffset?.ToString();
        set => ParallaxOffset = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlurAmount")]
    public string? BlurAmountString {
        get => BlurAmount?.ToString();
        set => BlurAmount = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SelfShadowStrength")]
    public string? SelfShadowStrengthString {
        get => SelfShadowStrength?.ToString();
        set => SelfShadowStrength = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("ObmDisplacement")]
    public string? ObmDisplacementString {
        get => ObmDisplacement?.ToString();
        set => ObmDisplacement = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("HeightBias")]
    public string? HeightBiasString {
        get => HeightBias?.ToString();
        set => HeightBias = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("DetailBumpTillingU")]
    public string? DetailBumpTillingUString {
        get => DetailBumpTillingU?.ToString();
        set => DetailBumpTillingU = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("DetailDiffuseScale")]
    public string? DetailDiffuseScaleString {
        get => DetailDiffuseScale?.ToString();
        set => DetailDiffuseScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("DetailBumpScale")]
    public string? DetailBumpScaleString {
        get => DetailBumpScale?.ToString();
        set => DetailBumpScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("DetailGlossScale")]
    public string? DetailGlossScaleString {
        get => DetailGlossScale?.ToString();
        set => DetailGlossScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("NormalsScale")]
    public string? NormalsScaleString {
        get => NormalsScale?.ToString();
        set => NormalsScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("Tilling")]
    public string? TillingString {
        get => Tilling?.ToString();
        set => Tilling = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FoamCrestAmount")]
    public string? FoamCrestAmountString {
        get => FoamCrestAmount?.ToString();
        set => FoamCrestAmount = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SubSurfaceScatteringScale")]
    public string? SubSurfaceScatteringScaleString {
        get => SubSurfaceScatteringScale?.ToString();
        set => SubSurfaceScatteringScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("RipplesNormalsScale")]
    public string? RipplesNormalsScaleString {
        get => RipplesNormalsScale?.ToString();
        set => RipplesNormalsScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("DetailNormalsScale")]
    public string? DetailNormalsScaleString {
        get => DetailNormalsScale?.ToString();
        set => DetailNormalsScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("DetailTilling")]
    public string? DetailTillingString {
        get => DetailTilling?.ToString();
        set => DetailTilling = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("ReflectionScale")]
    public string? ReflectionScaleString {
        get => ReflectionScale?.ToString();
        set => ReflectionScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FoamSoftIntersectionFactor")]
    public string? FoamSoftIntersectionFactorString {
        get => FoamSoftIntersectionFactor?.ToString();
        set => FoamSoftIntersectionFactor = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FoamTilling")]
    public string? FoamTillingString {
        get => FoamTilling?.ToString();
        set => FoamTilling = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("ReflectionBumpScale")]
    public string? ReflectionBumpScaleString {
        get => ReflectionBumpScale?.ToString();
        set => ReflectionBumpScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("RainTilling")]
    public string? RainTillingString {
        get => RainTilling?.ToString();
        set => RainTilling = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("PomDisplacement")]
    public string? PomDisplacementString {
        get => PomDisplacement?.ToString();
        set => PomDisplacement = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("ParallaxShadowMode")]
    public string? ParallaxShadowModeString {
        get => ParallaxShadowMode?.ToString();
        set => ParallaxShadowMode = value is null ? null : int.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendLayerAlphaConstant")]
    public string? BlendLayerAlphaConstantString {
        get => BlendLayerAlphaConstant?.ToString();
        set => BlendLayerAlphaConstant = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SpecularNoiseTiling")]
    public string? SpecularNoiseTilingString {
        get => SpecularNoiseTiling?.ToString();
        set => SpecularNoiseTiling = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("SpecularNoiseMode")]
    public string? SpecularNoiseModeString {
        get => SpecularNoiseMode?.ToString();
        set => SpecularNoiseMode = value is null ? null : int.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("CloudDistantTiling")]
    public string? CloudDistantTilingString {
        get => CloudDistantTiling?.ToString();
        set => CloudDistantTiling = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("CloudSkyColorMultiplier")]
    public string? CloudSkyColorMultiplierString {
        get => CloudSkyColorMultiplier?.ToString();
        set => CloudSkyColorMultiplier = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FogOpacity")]
    public string? FogOpacityString {
        get => FogOpacity?.ToString();
        set => FogOpacity = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("CloudDistantHeight")]
    public string? CloudDistantHeightString {
        get => CloudDistantHeight?.ToString();
        set => CloudDistantHeight = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FogBottomToTopColor")]
    public string? FogBottomToTopColorString {
        get => FogBottomToTopColor?.ToString();
        set => FogBottomToTopColor = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("FogHeight")]
    public string? FogHeightString {
        get => FogHeight?.ToString();
        set => FogHeight = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlurEnvironmentMap")]
    public string? BlurEnvironmentMapString {
        get => BlurEnvironmentMap?.ToString();
        set => BlurEnvironmentMap = value is null ? null : int.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("CloudDistantScrollSpeedX")]
    public string? CloudDistantScrollSpeedXString {
        get => CloudDistantScrollSpeedX?.ToString();
        set => CloudDistantScrollSpeedX = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("CloudOpacity")]
    public string? CloudOpacityString {
        get => CloudOpacity?.ToString();
        set => CloudOpacity = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("CloudDistantScrollSpeedY")]
    public string? CloudDistantScrollSpeedYString {
        get => CloudDistantScrollSpeedY?.ToString();
        set => CloudDistantScrollSpeedY = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("CubeRotateZ")]
    public string? CubeRotateZString {
        get => CubeRotateZ?.ToString();
        set => CubeRotateZ = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("CubeRotationSpeed")]
    public string? CubeRotationSpeedString {
        get => CubeRotationSpeed?.ToString();
        set => CubeRotationSpeed = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("CloudTranslucency")]
    public string? CloudTranslucencyString {
        get => CloudTranslucency?.ToString();
        set => CloudTranslucency = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("CubemapStretch")]
    public string? CubemapStretchString {
        get => CubemapStretch?.ToString();
        set => CubemapStretch = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("WaterDistantBump")]
    public string? WaterDistantBumpString {
        get => WaterDistantBump?.ToString();
        set => WaterDistantBump = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("WaterReflectionStrength")]
    public string? WaterReflectionStrengthString {
        get => WaterReflectionStrength?.ToString();
        set => WaterReflectionStrength = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("CubemapHorizonOffset")]
    public string? CubemapHorizonOffsetString {
        get => CubemapHorizonOffset?.ToString();
        set => CubemapHorizonOffset = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("GrainAmount")]
    public string? GrainAmountString {
        get => GrainAmount?.ToString();
        set => GrainAmount = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("DeformAmount")]
    public string? DeformAmountString {
        get => DeformAmount?.ToString();
        set => DeformAmount = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("DeformTillingY")]
    public string? DeformTillingYString {
        get => DeformTillingY?.ToString();
        set => DeformTillingY = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("GrainTilling")]
    public string? GrainTillingString {
        get => GrainTilling?.ToString();
        set => GrainTilling = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("InterlacingTilling")]
    public string? InterlacingTillingString {
        get => InterlacingTilling?.ToString();
        set => InterlacingTilling = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("DeformTillingX")]
    public string? DeformTillingXString {
        get => DeformTillingX?.ToString();
        set => DeformTillingX = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("GrainSaturation")]
    public string? GrainSaturationString {
        get => GrainSaturation?.ToString();
        set => GrainSaturation = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("InterlacingAmount")]
    public string? InterlacingAmountString {
        get => InterlacingAmount?.ToString();
        set => InterlacingAmount = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("VSyncFreq")]
    public string? VSyncFreqString {
        get => VSyncFreq?.ToString();
        set => VSyncFreq = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("ChromaShift")]
    public string? ChromaShiftString {
        get => ChromaShift?.ToString();
        set => ChromaShift = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("DeformFreq")]
    public string? DeformFreqString {
        get => DeformFreq?.ToString();
        set => DeformFreq = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendVectorBump")]
    public string? BlendVectorBumpString {
        get => BlendVectorBump?.ToString();
        set => BlendVectorBump = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendVectorCoverage")]
    public string? BlendVectorCoverageString {
        get => BlendVectorCoverage?.ToString();
        set => BlendVectorCoverage = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendVectorFeather")]
    public string? BlendVectorFeatherString {
        get => BlendVectorFeather?.ToString();
        set => BlendVectorFeather = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendVectorWorldX")]
    public string? BlendVectorWorldXString {
        get => BlendVectorWorldX?.ToString();
        set => BlendVectorWorldX = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendVectorWorldY")]
    public string? BlendVectorWorldYString {
        get => BlendVectorWorldY?.ToString();
        set => BlendVectorWorldY = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendVectorWorldZ")]
    public string? BlendVectorWorldZString {
        get => BlendVectorWorldZ?.ToString();
        set => BlendVectorWorldZ = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendHeightScale")]
    public string? BlendHeightScaleString {
        get => BlendHeightScale?.ToString();
        set => BlendHeightScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("WaterWallBumpScale")]
    public string? WaterWallBumpScaleString {
        get => WaterWallBumpScale?.ToString();
        set => WaterWallBumpScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("WaterWallFoamAmount")]
    public string? WaterWallFoamAmountString {
        get => WaterWallFoamAmount?.ToString();
        set => WaterWallFoamAmount = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("WaterWallParallaxOffset")]
    public string? WaterWallParallaxOffsetString {
        get => WaterWallParallaxOffset?.ToString();
        set => WaterWallParallaxOffset = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("WaterWallRefractionBumpScale")]
    public string? WaterWallRefractionBumpScaleString {
        get => WaterWallRefractionBumpScale?.ToString();
        set => WaterWallRefractionBumpScale = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("WaterWallSunshaftIntensity")]
    public string? WaterWallSunshaftIntensityString {
        get => WaterWallSunshaftIntensity?.ToString();
        set => WaterWallSunshaftIntensity = value is null ? null : float.Parse(value);
    }

    [JsonIgnore]
    [XmlAttribute("BlendMaskTiling")]
    public string? BlendMaskTilingString {
        get => BlendMaskTiling?.ToString();
        set => BlendMaskTiling = value is null ? null : float.Parse(value);
    }
}
