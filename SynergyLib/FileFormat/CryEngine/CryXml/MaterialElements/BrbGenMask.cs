using System;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[Flags]
public enum BrbGenMask : ulong {
    /// <summary>
    /// Use bump-map texture.
    /// </summary>
    [XmlEnum(Name = "BUMP_MAP")]
    BumpMap = 0x100000,

    /// <summary>
    /// Adds an approximated self-shadow derived from the height map.
    /// 
    /// "Cheap" parallax shadow
    /// </summary>
    [XmlEnum(Name = "OFFSETBUMPMAPPING_SHADOW")]
    OffsetBumpMappingShadow = 0x4000,

    /// <summary>
    /// Blend layer's normal map adds to base layer.
    /// </summary>
    [XmlEnum(Name = "BLENDNORMAL_ADD")]
    BlendBumpmapsAdditively = 0x200000000,

    /// <summary>
    /// Decal Map and/or Emissive Color sets rather than modulates diffuse map's color in glow pass.
    /// </summary>
    [XmlEnum(Name = "DECAL_GLOW")]
    GlowColorOccludesDiffuse = 0x40,

    /// <summary>
    /// Draw resolution independent iris and pupil.
    /// </summary>
    [XmlEnum(Name = "EYE_OVERLAY")]
    Iris = 0x4,

    /// <summary>
    /// Blend layer refracts base or detail layer based on blend layer's normal map.
    /// </summary>
    [XmlEnum(Name = "BLUR_REFRACTION")]
    BlendRefractsBaseDetail = 0x200000000000,

    /// <summary>
    /// Use Anisotropic specular version.
    /// </summary>
    [XmlEnum(Name = "ANISO_SPECULAR")]
    AnisotropicSpecular = 0x8000,

    /// <summary>
    /// Fetch Detail map as a traditional NORMALMAP. Otherwise, assumes Merged "dmap" has been assigned.
    /// </summary>
    [XmlEnum(Name = "DETAIL_TEXTURE_IS_NORMALMAP")]
    DetailMapIsNormalmap = 0x4000000,

    /// <summary>
    /// Use Diffuse map's alpha as Specular Level mask (a.k.a. Gloss map DifAlpha )...Specular map(s) override this.
    /// </summary>
    [XmlEnum(Name = "GLOSS_DIFFUSEALPHA")]
    SpecMaskInDiffuseAlpha = 0x20,

    /// <summary>
    /// Add a fresnel-like rim to an object using silhouette/outline color...useful for highlighting selected objects.
    /// </summary>
    [XmlEnum(Name = "RIM_HIGHLIGHT")]
    RimHighlight = 0x400000000000,

    /// <summary>
    /// Use worldspace vector to drive blending.
    /// </summary>
    [XmlEnum(Name = "BLENDVECTOR")]
    BlendVector = 0x2000000000,

    /// <summary>
    /// Assigned bump "bhg" map samples blend/parallax height, normals, and glossiness in a single fetch ( save using CryTiff preset "MergedDetailMap": R = Height, G = Normal X, B = Gloss, A = Normal Y ).
    ///
    /// Merged bump w/ height
    /// </summary>
    [XmlEnum(Name = "BLENDHEIGHT_DISPL")]
    BlendHeightDispl = 0x4000000000,

    /// <summary>
    /// Use bumpmap to influence worldspace vector blending (not compatible with Displacement Mapping).
    /// </summary>
    [XmlEnum(Name = "BLENDVECTOR_PS")]
    BlendVectorWithBumpmap = 0x2,

// TODO: LEAVES
    /// <summary>
    /// Glow with animated decal texture, but without glow pass.
    ///
    /// Glow+Decal w/o glow pass
    /// </summary>
    [XmlEnum(Name = "DECAL_ALPHAGLOW")]
    DecalAlphaGlow = 0x8000000000,

// TODO: MERGED_TEXTURES
// TODO: BLENDNORMAL
    /// <summary>
    /// Assigned bump "bsg" map includes includes translucency scatter (for skin), normals, and glossiness ( save using CryTiff preset "MergedDetailMap": R = Scatter, G = Normal X, B = Gloss, A = Normal Y ).
    ///
    /// Merged bump w/ scatter
    /// </summary>
    [XmlEnum(Name = "TEMP_SKIN")]
    TempSkin = 0x1000000,

// TODO: DEPTH_FOG
    /// <summary>
    /// Use custom secondary map as normal layer, vertex blended with mask in opacity against base material.
    /// </summary>
    [XmlEnum(Name = "DIRTLAYER")]
    Blendnormal = 0x200000,

    /// <summary>
    /// Emissive Color modulates glow color instead of providing an ambient boost...unaffected by Diffuse Color tinting.
    /// </summary>
    [XmlEnum(Name = "GLOW_EMISSIVE")]
    GlowFromEmissiveColor = 0x100000000,

// TODO: BLENDDIFFUSE
    /// <summary>
    /// Multiply vertex colors with specular result.
    /// </summary>
    [XmlEnum(Name = "VERTCOLORS_SPECULAR")]
    VertexModulatesSpecularColor = 0x400,

    /// <summary>
    /// Multiply vertex colors with diffuse result.
    /// </summary>
    [XmlEnum(Name = "VERTCOLORS")]
    VertexModulatesDiffuseColor = 0x400000,

    /// <summary>
    /// Calculate glow within opaque pass...presently negates use of decal map.
    /// </summary>
    [XmlEnum(Name = "ALPHAGLOW")]
    GlowWithoutGlowPass = 0x2000,

    /// <summary>
    /// Semi-transparent surfaces soft intersect the camera.
    /// </summary>
    [XmlEnum(Name = "NEAR_FADE")]
    NearFade = 0x1000000000000,

    /// <summary>
    /// Sweeping highlight to signify important object.
    /// </summary>
    [XmlEnum(Name = "HIGHLIGHT")]
    Highlight = 0x20000000,

    /// <summary>
    /// Use Diffuse map's alpha to mask Detail map.
    /// </summary>
    [XmlEnum(Name = "ALPHAMASK_DETAILMAP")]
    DetailMaskInDiffuseAlpha = 0x800000,

    /// <summary>
    /// Use separate environment map.
    /// </summary>
    [XmlEnum(Name = "ENVIRONMENT_MAP")]
    EnvironmentMap = 0x80,

    /// <summary>
    /// Use custom map as diffuse layer, vertex blended with mask in opacity against base material.
    /// </summary>
    [XmlEnum(Name = "BLENDLAYER")]
    Blenddiffuse = 0x100,

    /// <summary>
    /// Blend layer uses a user-set constant value instead of the blend diffuse map's alpha (default = 0).
    /// </summary>
    [XmlEnum(Name = "BLENDLAYER_ALPHA_CONSTANT")]
    BlendAlphaConstant = 0x800000000000,

// TODO: ALLOW_TESSELATION
    /// <summary>
    /// Use parallax occlusion mapping (requires height map (_displ) which will NOT work on WiiU).
    /// </summary>
    [XmlEnum(Name = "PARALLAX_OCCLUSION_MAPPING")]
    ParallaxOcclusionMapping = 0x8000000,

    /// <summary>
    /// Requires assigning a height map to Opacity or assigning AND enabling a Merged Bump, Gloss, Blend height map.
    ///
    /// "Cheap" parallax mapping
    /// </summary>
    [XmlEnum(Name = "OFFSETBUMPMAPPING")]
    OffsetBumpMapping = 0x20000,

    /// <summary>
    /// Use Opacity alpha as secondary height map OR when "bhg" is enabled (and not parallaxed), use [1]Custom map's bhg height channel as the blend driver.
    /// </summary>
    [XmlEnum(Name = "BLENDHEIGHT2_ENABLE")]
    BlendHeightmapsEnable = 0x40000000000,

// TODO: DETAIL_BENDING
    /// <summary>
    /// Use separate specular map...overrides Spec mask in Diffuse alpha.
    /// </summary>
    [XmlEnum(Name = "GLOSS_MAP")]
    GlossMap = 0x10,

    /// <summary>
    /// Use subsurface map as specular / shininess layer.
    /// </summary>
    [XmlEnum(Name = "BLENDSPECULAR")]
    Blendspecular = 0x200,

// TODO: TEMP_TERRAIN
    /// <summary>
    /// Use specular noise (for sand / snow).
    /// </summary>
    [XmlEnum(Name = "SPECULAR_NOISE")]
    SpecularNoise = 0x800000000,

// TODO: WATER_PASS
    /// <summary>
    /// Use Specular map's alpha as Glossiness mask (a.k.a. PerPixel Spec. Shinines)...meaningless without Specular map(s).
    /// </summary>
    [XmlEnum(Name = "SPECULARPOW_GLOSSALPHA")]
    GlossinessInSpecularAlpha = 0x800,

// TODO: BILINEAR_FP16
// TODO: TESSELLATION
// TODO: OCEAN_FOG_COLOR
    /// <summary>
    /// Invert blend mask map (Opacity or _displ depending on previous setting).
    /// </summary>
    [XmlEnum(Name = "BLENDHEIGHT_INVERT")]
    BlendMaskInvert = 0x400000000,

    /// <summary>
    /// Opaque waterfall-like effect.
    /// </summary>
    [XmlEnum(Name = "WATERWALL")]
    WaterWall = 0x100000000000,

    /// <summary>
    /// Use as Decal (not related to Decal Map).
    /// </summary>
    [XmlEnum(Name = "DECAL")]
    Decal = 0x2000000,

    /// <summary>
    /// Use separate blend map in opacity.
    /// </summary>
    [XmlEnum(Name = "BLEND_MAP")]
    BlendMap = 0x1000,

// TODO: TEMP_EYES
    [XmlEnum(Name = "DETAIL_TEXTURE_IS_SET")]
    DetailTextureIsSet = 0x10000,

    /// <summary>
    /// Force waterwall into water pass and use soft depth blending.
    /// </summary>
    [XmlEnum(Name = "REFRACTION_TRUE")]
    WaterWallTransparent = 0x1000000000,

    /// <summary>
    /// Sunlight is modulated for caustic effect.
    /// </summary>
    [XmlEnum(Name = "CAUSTICS")]
    Caustics = 0x80000000000,

    /// <summary>
    /// Draw outline around object using silhouette/outline color.
    /// </summary>
    [XmlEnum(Name = "OUTLINE")]
    Outline = 0x10000000000,
}
