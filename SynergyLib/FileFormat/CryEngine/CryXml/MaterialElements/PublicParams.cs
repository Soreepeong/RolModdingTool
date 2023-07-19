using System.Numerics;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[XmlRoot("PublicParams")]
public class PublicParams {
    public Vector3? SilhouetteColor;
    public Vector3? IrisColor;
    public Vector3? IndirectColor;
    public Vector3? WrapColor;
    public Vector3? BackDiffuse;
    public Vector3? TintColor;
    public Vector3? LiquidColor;
    public Vector3? FogColor;
    public float? Visibility;
    public float? IrisAspectRatio;
    public float? IrisRotate;
    public float? IrisSize;
    public float? EyeSadFactor;
    public float? PupilScale;
    public float? SilhouetteIntensity;
    public float? FresnelPower;
    public float? FresnelScale;
    public float? FresnelBias;
    public float? IrisParallaxOffset;
    public float? EyeRotateVertical;
    public float? EyeRotateHorizontal;
    public float? EyeRotationMultiplier;
    public float? Metalness;
    public float? WrapDiffuse;
    public float? BendDetailLeafAmplitude;
    public float? BendDetailFrequency;
    public float? BendDetailBranchAmplitude;
    public float? BlendFalloff;
    public float? BlendLayer2Tiling;
    public float? BlendFactor;
    public float? BlendWithTerrainAmount;
    public float? BlendMaskRotation;
    public float? BlendNormalAddFactor;
    public float? BackDiffuseMultiplier;
    public float? BackShadowBias;
    public float? BackViewDep;
    public float? CapOpacityFalloff;
    public float? BackLightScale;
    public float? BumpScale;
    public float? IndexOfRefraction;
    public float? TintCloudiness;
    public float? GlossFromDiffuseContrast;
    public float? GlossFromDiffuseOffset;
    public float? GlossFromDiffuseAmount;
    public float? GlossFromDiffuseBrightness;
    public int? SpecMapChannelR;
    public int? SpecMapChannelG;
    public int? SpecMapChannelB;
    public int? GlossMapChannelR;
    public int? GlossMapChannelG;
    public int? GlossMapChannelB;
    public float? FlowTilling;
    public float? GlowRimFactor;
    public float? ShieldOn;
    public float? ShieldHit;
    public float? FlowSpeed;
    public float? FogDensity;
    public float? HdrDynamic;
    public float? AmbientMultiplier;
    public float? SoftIntersectionFactor;
    public float? SunMultiplier;
    public float? TillingLayer0;
    public float? SpeedLayer0;
    public float? SpeedLayer1;
    public float? TillingLayer1;
    public float? FoamMultiplier;
    public float? RedShift;
    public float? RefractionBumpScale;
    public float? ReflectionAmount;
    public float? ReflectAmount;
    public float? BumpScaleLayer1;
    public float? FoamDeform;
    public float? FoamAmount;
    public float? BumpScaleLayer0;
    public float? AnimAmplitudeWav0;
    public float? FuzzynessSpread;
    public float? FuzzynessStrength;
    public float? FuzzynessSmoothness;
    public float? FuzzynessSaturation;
    public float? DiffuseExponent;
    public float? AnimFrequency;
    public float? AnimAmplitudeWav2;
    public float? AnimPhase;
    public float? Noise;
    public float? NoiseScale;
    public float? ScaleXz;
    public float? ScaleXzEnd;
    public float? NoiseSpeed;
    public float? SpecFromDiffAlphaBias;
    public float? SpecFromDiffAlphaMult;
    public float? FogCutoffEnd;
    public float? BungeeWobbleLoose;
    public float? BungeeWobbleMult;
    public float? BungeeTension;
    public float? BungeeWobbleTense;
    public int? BungeeMode;
    public float? BungeeWobble;
    public float? GlowCausticFade;
    public float? AnisotropicShape;
    public float? StartRadius;
    public float? EndRadius;
    public Vector3? StartColor;
    public Vector3? EndColor;
    public float? FinalMultiplier;
    public float? BeamLength;
    public float? BeamOriginFade;
    public float? ViewDependencyFactor;
    public float? OrigLength;
    public float? OrigWidth;
    public int? BlendLayerToDefault;
    public float? ParallaxOffset;
    public float? BlurAmount;
    public float? SelfShadowStrength;
    public float? ObmDisplacement;
    public float? HeightBias;
    public float? DetailBumpTillingU;
    public float? DetailDiffuseScale;
    public float? DetailBumpScale;
    public float? DetailGlossScale;
    public float? NormalsScale;
    public float? Tilling;
    public float? FoamCrestAmount;
    public float? SubSurfaceScatteringScale;
    public float? RipplesNormalsScale;
    public float? DetailNormalsScale;
    public float? DetailTilling;
    public float? ReflectionScale;
    public float? FoamSoftIntersectionFactor;
    public float? FoamTilling;
    public float? ReflectionBumpScale;
    public float? RainTilling;
    public float? PomDisplacement;
    public int? ParallaxShadowMode;
    public float? BlendLayerAlphaConstant;
    public float? SpecularNoiseTiling;
    public int? SpecularNoiseMode;
    public float? CloudDistantTiling;
    public float? CloudSkyColorMultiplier;
    public float? FogOpacity;
    public float? CloudDistantHeight;
    public float? FogBottomToTopColor;
    public float? FogHeight;
    public int? BlurEnvironmentMap;
    public float? CloudDistantScrollSpeedX;
    public float? CloudOpacity;
    public float? CloudDistantScrollSpeedY;
    public float? CubeRotateZ;
    public float? CubeRotationSpeed;
    public float? CloudTranslucency;
    public float? CubemapStretch;
    public float? WaterDistantBump;
    public float? WaterReflectionStrength;
    public float? CubemapHorizonOffset;
    public float? GrainAmount;
    public float? DeformAmount;
    public float? DeformTillingY;
    public float? GrainTilling;
    public float? InterlacingTilling;
    public float? DeformTillingX;
    public float? GrainSaturation;
    public float? InterlacingAmount;
    public float? VSyncFreq;
    public float? ChromaShift;
    public float? DeformFreq;
    public float? BlendVectorBump;
    public float? BlendVectorCoverage;
    public float? BlendVectorFeather;
    public float? BlendVectorWorldX;
    public float? BlendVectorWorldY;
    public float? BlendVectorWorldZ;
    public float? BlendHeightScale;
    public float? WaterWallBumpScale;
    public float? WaterWallFoamAmount;
    public float? WaterWallParallaxOffset;
    public float? WaterWallRefractionBumpScale;
    public float? WaterWallSunshaftIntensity;
    public float? BlendMaskTiling;

    [XmlAttribute("SilhouetteColor")]
    public string? SilhouetteColorString {
        get => SilhouetteColor.ToXmlValue();
        set => SilhouetteColor = value?.XmlToVector3();
    }

    [XmlAttribute("IrisColor")]
    public string? IrisColorString {
        get => IrisColor.ToXmlValue();
        set => IrisColor = value?.XmlToVector3();
    }

    [XmlAttribute("IndirectColor")]
    public string? IndirectColorString {
        get => IndirectColor.ToXmlValue();
        set => IndirectColor = value?.XmlToVector3();
    }

    [XmlAttribute("WrapColor")]
    public string? WrapColorString {
        get => WrapColor.ToXmlValue();
        set => WrapColor = value?.XmlToVector3();
    }

    [XmlAttribute("BackDiffuse")]
    public string? BackDiffuseString {
        get => BackDiffuse.ToXmlValue();
        set => BackDiffuse = value?.XmlToVector3();
    }

    [XmlAttribute("TintColor")]
    public string? TintColorString {
        get => TintColor.ToXmlValue();
        set => TintColor = value?.XmlToVector3();
    }

    [XmlAttribute("LiquidColor")]
    public string? LiquidColorString {
        get => LiquidColor.ToXmlValue();
        set => LiquidColor = value?.XmlToVector3();
    }

    [XmlAttribute("FogColor")]
    public string? FogColorString {
        get => FogColor.ToXmlValue();
        set => FogColor = value?.XmlToVector3();
    }

    [XmlAttribute("Visibility")]
    public string? VisibilityString {
        get => Visibility?.ToString();
        set => Visibility = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("IrisAspectRatio")]
    public string? IrisAspectRatioString {
        get => IrisAspectRatio?.ToString();
        set => IrisAspectRatio = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("IrisRotate")]
    public string? IrisRotateString {
        get => IrisRotate?.ToString();
        set => IrisRotate = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("IrisSize")]
    public string? IrisSizeString {
        get => IrisSize?.ToString();
        set => IrisSize = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("EyeSadFactor")]
    public string? EyeSadFactorString {
        get => EyeSadFactor?.ToString();
        set => EyeSadFactor = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("PupilScale")]
    public string? PupilScaleString {
        get => PupilScale?.ToString();
        set => PupilScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("SilhouetteIntensity")]
    public string? SilhouetteIntensityString {
        get => SilhouetteIntensity?.ToString();
        set => SilhouetteIntensity = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FresnelPower")]
    public string? FresnelPowerString {
        get => FresnelPower?.ToString();
        set => FresnelPower = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FresnelScale")]
    public string? FresnelScaleString {
        get => FresnelScale?.ToString();
        set => FresnelScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FresnelBias")]
    public string? FresnelBiasString {
        get => FresnelBias?.ToString();
        set => FresnelBias = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("IrisParallaxOffset")]
    public string? IrisParallaxOffsetString {
        get => IrisParallaxOffset?.ToString();
        set => IrisParallaxOffset = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("EyeRotateVertical")]
    public string? EyeRotateVerticalString {
        get => EyeRotateVertical?.ToString();
        set => EyeRotateVertical = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("EyeRotateHorizontal")]
    public string? EyeRotateHorizontalString {
        get => EyeRotateHorizontal?.ToString();
        set => EyeRotateHorizontal = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("EyeRotationMultiplier")]
    public string? EyeRotationMultiplierString {
        get => EyeRotationMultiplier?.ToString();
        set => EyeRotationMultiplier = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("Metalness")]
    public string? MetalnessString {
        get => Metalness?.ToString();
        set => Metalness = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("WrapDiffuse")]
    public string? WrapDiffuseString {
        get => WrapDiffuse?.ToString();
        set => WrapDiffuse = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("bendDetailLeafAmplitude")]
    public string? BendDetailLeafAmplitudeString {
        get => BendDetailLeafAmplitude?.ToString();
        set => BendDetailLeafAmplitude = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("bendDetailFrequency")]
    public string? BendDetailFrequencyString {
        get => BendDetailFrequency?.ToString();
        set => BendDetailFrequency = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("bendDetailBranchAmplitude")]
    public string? BendDetailBranchAmplitudeString {
        get => BendDetailBranchAmplitude?.ToString();
        set => BendDetailBranchAmplitude = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendFalloff")]
    public string? BlendFalloffString {
        get => BlendFalloff?.ToString();
        set => BlendFalloff = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendLayer2Tiling")]
    public string? BlendLayer2TilingString {
        get => BlendLayer2Tiling?.ToString();
        set => BlendLayer2Tiling = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendFactor")]
    public string? BlendFactorString {
        get => BlendFactor?.ToString();
        set => BlendFactor = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("blendWithTerrainAmount")]
    public string? BlendWithTerrainAmountString {
        get => BlendWithTerrainAmount?.ToString();
        set => BlendWithTerrainAmount = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendMaskRotation")]
    public string? BlendMaskRotationString {
        get => BlendMaskRotation?.ToString();
        set => BlendMaskRotation = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendNormalAddFactor")]
    public string? BlendNormalAddFactorString {
        get => BlendNormalAddFactor?.ToString();
        set => BlendNormalAddFactor = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BackDiffuseMultiplier")]
    public string? BackDiffuseMultiplierString {
        get => BackDiffuseMultiplier?.ToString();
        set => BackDiffuseMultiplier = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BackShadowBias")]
    public string? BackShadowBiasString {
        get => BackShadowBias?.ToString();
        set => BackShadowBias = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BackViewDep")]
    public string? BackViewDepString {
        get => BackViewDep?.ToString();
        set => BackViewDep = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("CapOpacityFalloff")]
    public string? CapOpacityFalloffString {
        get => CapOpacityFalloff?.ToString();
        set => CapOpacityFalloff = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BackLightScale")]
    public string? BackLightScaleString {
        get => BackLightScale?.ToString();
        set => BackLightScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BumpScale")]
    public string? BumpScaleString {
        get => BumpScale?.ToString();
        set => BumpScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("IndexOfRefraction")]
    public string? IndexOfRefractionString {
        get => IndexOfRefraction?.ToString();
        set => IndexOfRefraction = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("TintCloudiness")]
    public string? TintCloudinessString {
        get => TintCloudiness?.ToString();
        set => TintCloudiness = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("GlossFromDiffuseContrast")]
    public string? GlossFromDiffuseContrastString {
        get => GlossFromDiffuseContrast?.ToString();
        set => GlossFromDiffuseContrast = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("GlossFromDiffuseOffset")]
    public string? GlossFromDiffuseOffsetString {
        get => GlossFromDiffuseOffset?.ToString();
        set => GlossFromDiffuseOffset = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("GlossFromDiffuseAmount")]
    public string? GlossFromDiffuseAmountString {
        get => GlossFromDiffuseAmount?.ToString();
        set => GlossFromDiffuseAmount = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("GlossFromDiffuseBrightness")]
    public string? GlossFromDiffuseBrightnessString {
        get => GlossFromDiffuseBrightness?.ToString();
        set => GlossFromDiffuseBrightness = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("SpecMapChannelR")]
    public string? SpecMapChannelRString {
        get => SpecMapChannelR?.ToString();
        set => SpecMapChannelR = value is null ? null : int.Parse(value);
    }

    [XmlAttribute("SpecMapChannelG")]
    public string? SpecMapChannelGString {
        get => SpecMapChannelG?.ToString();
        set => SpecMapChannelG = value is null ? null : int.Parse(value);
    }

    [XmlAttribute("SpecMapChannelB")]
    public string? SpecMapChannelBString {
        get => SpecMapChannelB?.ToString();
        set => SpecMapChannelB = value is null ? null : int.Parse(value);
    }

    [XmlAttribute("GlossMapChannelR")]
    public string? GlossMapChannelRString {
        get => GlossMapChannelR?.ToString();
        set => GlossMapChannelR = value is null ? null : int.Parse(value);
    }

    [XmlAttribute("GlossMapChannelG")]
    public string? GlossMapChannelGString {
        get => GlossMapChannelG?.ToString();
        set => GlossMapChannelG = value is null ? null : int.Parse(value);
    }

    [XmlAttribute("GlossMapChannelB")]
    public string? GlossMapChannelBString {
        get => GlossMapChannelB?.ToString();
        set => GlossMapChannelB = value is null ? null : int.Parse(value);
    }

    [XmlAttribute("FlowTilling")]
    public string? FlowTillingString {
        get => FlowTilling?.ToString();
        set => FlowTilling = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("GlowRimFactor")]
    public string? GlowRimFactorString {
        get => GlowRimFactor?.ToString();
        set => GlowRimFactor = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("ShieldOn")]
    public string? ShieldOnString {
        get => ShieldOn?.ToString();
        set => ShieldOn = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("ShieldHit")]
    public string? ShieldHitString {
        get => ShieldHit?.ToString();
        set => ShieldHit = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FlowSpeed")]
    public string? FlowSpeedString {
        get => FlowSpeed?.ToString();
        set => FlowSpeed = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FogDensity")]
    public string? FogDensityString {
        get => FogDensity?.ToString();
        set => FogDensity = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("HDRDynamic")]
    public string? HdrDynamicString {
        get => HdrDynamic?.ToString();
        set => HdrDynamic = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("AmbientMultiplier")]
    public string? AmbientMultiplierString {
        get => AmbientMultiplier?.ToString();
        set => AmbientMultiplier = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("SoftIntersectionFactor")]
    public string? SoftIntersectionFactorString {
        get => SoftIntersectionFactor?.ToString();
        set => SoftIntersectionFactor = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("SunMultiplier")]
    public string? SunMultiplierString {
        get => SunMultiplier?.ToString();
        set => SunMultiplier = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("TillingLayer0")]
    public string? TillingLayer0String {
        get => TillingLayer0?.ToString();
        set => TillingLayer0 = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("SpeedLayer0")]
    public string? SpeedLayer0String {
        get => SpeedLayer0?.ToString();
        set => SpeedLayer0 = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("SpeedLayer1")]
    public string? SpeedLayer1String {
        get => SpeedLayer1?.ToString();
        set => SpeedLayer1 = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("TillingLayer1")]
    public string? TillingLayer1String {
        get => TillingLayer1?.ToString();
        set => TillingLayer1 = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FoamMultiplier")]
    public string? FoamMultiplierString {
        get => FoamMultiplier?.ToString();
        set => FoamMultiplier = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("RedShift")]
    public string? RedShiftString {
        get => RedShift?.ToString();
        set => RedShift = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("RefractionBumpScale")]
    public string? RefractionBumpScaleString {
        get => RefractionBumpScale?.ToString();
        set => RefractionBumpScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("ReflectionAmount")]
    public string? ReflectionAmountString {
        get => ReflectionAmount?.ToString();
        set => ReflectionAmount = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("ReflectAmount")]
    public string? ReflectAmountString {
        get => ReflectAmount?.ToString();
        set => ReflectAmount = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BumpScaleLayer1")]
    public string? BumpScaleLayer1String {
        get => BumpScaleLayer1?.ToString();
        set => BumpScaleLayer1 = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FoamDeform")]
    public string? FoamDeformString {
        get => FoamDeform?.ToString();
        set => FoamDeform = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FoamAmount")]
    public string? FoamAmountString {
        get => FoamAmount?.ToString();
        set => FoamAmount = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BumpScaleLayer0")]
    public string? BumpScaleLayer0String {
        get => BumpScaleLayer0?.ToString();
        set => BumpScaleLayer0 = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("AnimAmplitudeWav0")]
    public string? AnimAmplitudeWav0String {
        get => AnimAmplitudeWav0?.ToString();
        set => AnimAmplitudeWav0 = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FuzzynessSpread")]
    public string? FuzzynessSpreadString {
        get => FuzzynessSpread?.ToString();
        set => FuzzynessSpread = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FuzzynessStrength")]
    public string? FuzzynessStrengthString {
        get => FuzzynessStrength?.ToString();
        set => FuzzynessStrength = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FuzzynessSmoothness")]
    public string? FuzzynessSmoothnessString {
        get => FuzzynessSmoothness?.ToString();
        set => FuzzynessSmoothness = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FuzzynessSaturation")]
    public string? FuzzynessSaturationString {
        get => FuzzynessSaturation?.ToString();
        set => FuzzynessSaturation = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("DiffuseExponent")]
    public string? DiffuseExponentString {
        get => DiffuseExponent?.ToString();
        set => DiffuseExponent = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("AnimFrequency")]
    public string? AnimFrequencyString {
        get => AnimFrequency?.ToString();
        set => AnimFrequency = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("AnimAmplitudeWav2")]
    public string? AnimAmplitudeWav2String {
        get => AnimAmplitudeWav2?.ToString();
        set => AnimAmplitudeWav2 = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("AnimPhase")]
    public string? AnimPhaseString {
        get => AnimPhase?.ToString();
        set => AnimPhase = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("Noise")]
    public string? NoiseString {
        get => Noise?.ToString();
        set => Noise = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("NoiseScale")]
    public string? NoiseScaleString {
        get => NoiseScale?.ToString();
        set => NoiseScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("ScaleXZ")]
    public string? ScaleXzString {
        get => ScaleXz?.ToString();
        set => ScaleXz = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("ScaleXZend")]
    public string? ScaleXzEndString {
        get => ScaleXzEnd?.ToString();
        set => ScaleXzEnd = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("NoiseSpeed")]
    public string? NoiseSpeedString {
        get => NoiseSpeed?.ToString();
        set => NoiseSpeed = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("SpecFromDiffAlphaBias")]
    public string? SpecFromDiffAlphaBiasString {
        get => SpecFromDiffAlphaBias?.ToString();
        set => SpecFromDiffAlphaBias = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("SpecFromDiffAlphaMult")]
    public string? SpecFromDiffAlphaMultString {
        get => SpecFromDiffAlphaMult?.ToString();
        set => SpecFromDiffAlphaMult = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FogCutoffEnd")]
    public string? FogCutoffEndString {
        get => FogCutoffEnd?.ToString();
        set => FogCutoffEnd = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BungeeWobbleLoose")]
    public string? BungeeWobbleLooseString {
        get => BungeeWobbleLoose?.ToString();
        set => BungeeWobbleLoose = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BungeeWobbleMult")]
    public string? BungeeWobbleMultString {
        get => BungeeWobbleMult?.ToString();
        set => BungeeWobbleMult = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BungeeTension")]
    public string? BungeeTensionString {
        get => BungeeTension?.ToString();
        set => BungeeTension = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BungeeWobbleTense")]
    public string? BungeeWobbleTenseString {
        get => BungeeWobbleTense?.ToString();
        set => BungeeWobbleTense = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BungeeMode")]
    public string? BungeeModeString {
        get => BungeeMode?.ToString();
        set => BungeeMode = value is null ? null : int.Parse(value);
    }

    [XmlAttribute("BungeeWobble")]
    public string? BungeeWobbleString {
        get => BungeeWobble?.ToString();
        set => BungeeWobble = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("GlowCausticFade")]
    public string? GlowCausticFadeString {
        get => GlowCausticFade?.ToString();
        set => GlowCausticFade = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("AnisotropicShape")]
    public string? AnisotropicShapeString {
        get => AnisotropicShape?.ToString();
        set => AnisotropicShape = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("StartRadius")]
    public string? StartRadiusString {
        get => StartRadius?.ToString();
        set => StartRadius = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("EndRadius")]
    public string? EndRadiusString {
        get => EndRadius?.ToString();
        set => EndRadius = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("StartColor")]
    public string? StartColorString {
        get => StartColor?.ToXmlValue();
        set => StartColor = value?.XmlToVector3();
    }

    [XmlAttribute("EndColor")]
    public string? EndColorString {
        get => EndColor?.ToString();
        set => EndColor = value?.XmlToVector3();
    }

    [XmlAttribute("FinalMultiplier")]
    public string? FinalMultiplierString {
        get => FinalMultiplier?.ToString();
        set => FinalMultiplier = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BeamLength")]
    public string? BeamLengthString {
        get => BeamLength?.ToString();
        set => BeamLength = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BeamOriginFade")]
    public string? BeamOriginFadeString {
        get => BeamOriginFade?.ToString();
        set => BeamOriginFade = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("viewDependencyFactor")]
    public string? ViewDependencyFactorString {
        get => ViewDependencyFactor?.ToString();
        set => ViewDependencyFactor = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("OrigLength")]
    public string? OrigLengthString {
        get => OrigLength?.ToString();
        set => OrigLength = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("OrigWidth")]
    public string? OrigWidthString {
        get => OrigWidth?.ToString();
        set => OrigWidth = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendLayerToDefault")]
    public string? BlendLayerToDefaultString {
        get => BlendLayerToDefault?.ToString();
        set => BlendLayerToDefault = value is null ? null : int.Parse(value);
    }

    [XmlAttribute("ParallaxOffset")]
    public string? ParallaxOffsetString {
        get => ParallaxOffset?.ToString();
        set => ParallaxOffset = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlurAmount")]
    public string? BlurAmountString {
        get => BlurAmount?.ToString();
        set => BlurAmount = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("SelfShadowStrength")]
    public string? SelfShadowStrengthString {
        get => SelfShadowStrength?.ToString();
        set => SelfShadowStrength = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("ObmDisplacement")]
    public string? ObmDisplacementString {
        get => ObmDisplacement?.ToString();
        set => ObmDisplacement = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("HeightBias")]
    public string? HeightBiasString {
        get => HeightBias?.ToString();
        set => HeightBias = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("DetailBumpTillingU")]
    public string? DetailBumpTillingUString {
        get => DetailBumpTillingU?.ToString();
        set => DetailBumpTillingU = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("DetailDiffuseScale")]
    public string? DetailDiffuseScaleString {
        get => DetailDiffuseScale?.ToString();
        set => DetailDiffuseScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("DetailBumpScale")]
    public string? DetailBumpScaleString {
        get => DetailBumpScale?.ToString();
        set => DetailBumpScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("DetailGlossScale")]
    public string? DetailGlossScaleString {
        get => DetailGlossScale?.ToString();
        set => DetailGlossScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("NormalsScale")]
    public string? NormalsScaleString {
        get => NormalsScale?.ToString();
        set => NormalsScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("Tilling")]
    public string? TillingString {
        get => Tilling?.ToString();
        set => Tilling = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FoamCrestAmount")]
    public string? FoamCrestAmountString {
        get => FoamCrestAmount?.ToString();
        set => FoamCrestAmount = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("SubSurfaceScatteringScale")]
    public string? SubSurfaceScatteringScaleString {
        get => SubSurfaceScatteringScale?.ToString();
        set => SubSurfaceScatteringScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("RipplesNormalsScale")]
    public string? RipplesNormalsScaleString {
        get => RipplesNormalsScale?.ToString();
        set => RipplesNormalsScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("DetailNormalsScale")]
    public string? DetailNormalsScaleString {
        get => DetailNormalsScale?.ToString();
        set => DetailNormalsScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("DetailTilling")]
    public string? DetailTillingString {
        get => DetailTilling?.ToString();
        set => DetailTilling = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("ReflectionScale")]
    public string? ReflectionScaleString {
        get => ReflectionScale?.ToString();
        set => ReflectionScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FoamSoftIntersectionFactor")]
    public string? FoamSoftIntersectionFactorString {
        get => FoamSoftIntersectionFactor?.ToString();
        set => FoamSoftIntersectionFactor = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FoamTilling")]
    public string? FoamTillingString {
        get => FoamTilling?.ToString();
        set => FoamTilling = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("ReflectionBumpScale")]
    public string? ReflectionBumpScaleString {
        get => ReflectionBumpScale?.ToString();
        set => ReflectionBumpScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("RainTilling")]
    public string? RainTillingString {
        get => RainTilling?.ToString();
        set => RainTilling = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("PomDisplacement")]
    public string? PomDisplacementString {
        get => PomDisplacement?.ToString();
        set => PomDisplacement = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("ParallaxShadowMode")]
    public string? ParallaxShadowModeString {
        get => ParallaxShadowMode?.ToString();
        set => ParallaxShadowMode = value is null ? null : int.Parse(value);
    }

    [XmlAttribute("BlendLayerAlphaConstant")]
    public string? BlendLayerAlphaConstantString {
        get => BlendLayerAlphaConstant?.ToString();
        set => BlendLayerAlphaConstant = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("SpecularNoiseTiling")]
    public string? SpecularNoiseTilingString {
        get => SpecularNoiseTiling?.ToString();
        set => SpecularNoiseTiling = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("SpecularNoiseMode")]
    public string? SpecularNoiseModeString {
        get => SpecularNoiseMode?.ToString();
        set => SpecularNoiseMode = value is null ? null : int.Parse(value);
    }

    [XmlAttribute("CloudDistantTiling")]
    public string? CloudDistantTilingString {
        get => CloudDistantTiling?.ToString();
        set => CloudDistantTiling = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("CloudSkyColorMultiplier")]
    public string? CloudSkyColorMultiplierString {
        get => CloudSkyColorMultiplier?.ToString();
        set => CloudSkyColorMultiplier = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FogOpacity")]
    public string? FogOpacityString {
        get => FogOpacity?.ToString();
        set => FogOpacity = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("CloudDistantHeight")]
    public string? CloudDistantHeightString {
        get => CloudDistantHeight?.ToString();
        set => CloudDistantHeight = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FogBottomToTopColor")]
    public string? FogBottomToTopColorString {
        get => FogBottomToTopColor?.ToString();
        set => FogBottomToTopColor = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("FogHeight")]
    public string? FogHeightString {
        get => FogHeight?.ToString();
        set => FogHeight = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlurEnvironmentMap")]
    public string? BlurEnvironmentMapString {
        get => BlurEnvironmentMap?.ToString();
        set => BlurEnvironmentMap = value is null ? null : int.Parse(value);
    }

    [XmlAttribute("CloudDistantScrollSpeedX")]
    public string? CloudDistantScrollSpeedXString {
        get => CloudDistantScrollSpeedX?.ToString();
        set => CloudDistantScrollSpeedX = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("CloudOpacity")]
    public string? CloudOpacityString {
        get => CloudOpacity?.ToString();
        set => CloudOpacity = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("CloudDistantScrollSpeedY")]
    public string? CloudDistantScrollSpeedYString {
        get => CloudDistantScrollSpeedY?.ToString();
        set => CloudDistantScrollSpeedY = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("CubeRotateZ")]
    public string? CubeRotateZString {
        get => CubeRotateZ?.ToString();
        set => CubeRotateZ = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("CubeRotationSpeed")]
    public string? CubeRotationSpeedString {
        get => CubeRotationSpeed?.ToString();
        set => CubeRotationSpeed = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("CloudTranslucency")]
    public string? CloudTranslucencyString {
        get => CloudTranslucency?.ToString();
        set => CloudTranslucency = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("CubemapStretch")]
    public string? CubemapStretchString {
        get => CubemapStretch?.ToString();
        set => CubemapStretch = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("WaterDistantBump")]
    public string? WaterDistantBumpString {
        get => WaterDistantBump?.ToString();
        set => WaterDistantBump = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("WaterReflectionStrength")]
    public string? WaterReflectionStrengthString {
        get => WaterReflectionStrength?.ToString();
        set => WaterReflectionStrength = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("CubemapHorizonOffset")]
    public string? CubemapHorizonOffsetString {
        get => CubemapHorizonOffset?.ToString();
        set => CubemapHorizonOffset = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("GrainAmount")]
    public string? GrainAmountString {
        get => GrainAmount?.ToString();
        set => GrainAmount = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("DeformAmount")]
    public string? DeformAmountString {
        get => DeformAmount?.ToString();
        set => DeformAmount = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("DeformTillingY")]
    public string? DeformTillingYString {
        get => DeformTillingY?.ToString();
        set => DeformTillingY = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("GrainTilling")]
    public string? GrainTillingString {
        get => GrainTilling?.ToString();
        set => GrainTilling = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("InterlacingTilling")]
    public string? InterlacingTillingString {
        get => InterlacingTilling?.ToString();
        set => InterlacingTilling = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("DeformTillingX")]
    public string? DeformTillingXString {
        get => DeformTillingX?.ToString();
        set => DeformTillingX = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("GrainSaturation")]
    public string? GrainSaturationString {
        get => GrainSaturation?.ToString();
        set => GrainSaturation = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("InterlacingAmount")]
    public string? InterlacingAmountString {
        get => InterlacingAmount?.ToString();
        set => InterlacingAmount = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("VSyncFreq")]
    public string? VSyncFreqString {
        get => VSyncFreq?.ToString();
        set => VSyncFreq = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("ChromaShift")]
    public string? ChromaShiftString {
        get => ChromaShift?.ToString();
        set => ChromaShift = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("DeformFreq")]
    public string? DeformFreqString {
        get => DeformFreq?.ToString();
        set => DeformFreq = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendVectorBump")]
    public string? BlendVectorBumpString {
        get => BlendVectorBump?.ToString();
        set => BlendVectorBump = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendVectorCoverage")]
    public string? BlendVectorCoverageString {
        get => BlendVectorCoverage?.ToString();
        set => BlendVectorCoverage = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendVectorFeather")]
    public string? BlendVectorFeatherString {
        get => BlendVectorFeather?.ToString();
        set => BlendVectorFeather = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendVectorWorldX")]
    public string? BlendVectorWorldXString {
        get => BlendVectorWorldX?.ToString();
        set => BlendVectorWorldX = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendVectorWorldY")]
    public string? BlendVectorWorldYString {
        get => BlendVectorWorldY?.ToString();
        set => BlendVectorWorldY = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendVectorWorldZ")]
    public string? BlendVectorWorldZString {
        get => BlendVectorWorldZ?.ToString();
        set => BlendVectorWorldZ = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendHeightScale")]
    public string? BlendHeightScaleString {
        get => BlendHeightScale?.ToString();
        set => BlendHeightScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("WaterWallBumpScale")]
    public string? WaterWallBumpScaleString {
        get => WaterWallBumpScale?.ToString();
        set => WaterWallBumpScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("WaterWallFoamAmount")]
    public string? WaterWallFoamAmountString {
        get => WaterWallFoamAmount?.ToString();
        set => WaterWallFoamAmount = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("WaterWallParallaxOffset")]
    public string? WaterWallParallaxOffsetString {
        get => WaterWallParallaxOffset?.ToString();
        set => WaterWallParallaxOffset = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("WaterWallRefractionBumpScale")]
    public string? WaterWallRefractionBumpScaleString {
        get => WaterWallRefractionBumpScale?.ToString();
        set => WaterWallRefractionBumpScale = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("WaterWallSunshaftIntensity")]
    public string? WaterWallSunshaftIntensityString {
        get => WaterWallSunshaftIntensity?.ToString();
        set => WaterWallSunshaftIntensity = value is null ? null : float.Parse(value);
    }

    [XmlAttribute("BlendMaskTiling")]
    public string? BlendMaskTilingString {
        get => BlendMaskTiling?.ToString();
        set => BlendMaskTiling = value is null ? null : float.Parse(value);
    }
}
