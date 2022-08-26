namespace VT2AssetLib.Stingray.Resources;

internal sealed class BonesResource
{
    public uint BoneCount { get; set; }

    public uint LodLevelCount { get; set; }

    public IDString32[] BoneNameHashes { get; set; } = null!;

    public uint[] LodLevels { get; set; } = null!;

    public string[] BoneNames { get; set; } = null!;
}