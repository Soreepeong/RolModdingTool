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
    public string? ScaleXZString {
        get => ScaleXz?.ToString();
        set => ScaleXz = value is null ? null : float.Parse(value);
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
}
