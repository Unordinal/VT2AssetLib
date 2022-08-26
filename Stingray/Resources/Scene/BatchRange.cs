namespace VT2AssetLib.Stingray.Resources.Scene;

internal class BatchRange
{
    public uint MaterialIndex { get; set; }

    /// <remarks>
    /// Per-triangle; multiply by 3 to get the index.
    /// </remarks>
    public uint Start { get; set; }

    /// <remarks>
    /// Per-triangle; multiply by 3 to get the index.
    /// </remarks>
    public uint Size { get; set; }

    public uint BoneSet { get; set; }
}