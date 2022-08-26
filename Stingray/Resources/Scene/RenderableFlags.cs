namespace VT2AssetLib.Stingray.Resources.Scene;

[Flags]
public enum RenderableFlags : uint
{
    ViewportVisible = 0x1,
    ShadowCaster = 0x2,
    CullingAlwaysVisible = 0x4,
    CullingDisabled = 0x8,
    Occluder = 0x10,
    SurfaceQueries = 0x20,
    StaticShadowCaster = 0x40,
    UmbraTarget = 0x80,
    AlwaysKeep = 0x100,
    UmbraV2 = 0x200,
    DoubleSided = 0x400,
    UmbraOccluder = 0x800,
    ParticleMeshSpawning = 0x1000,
}