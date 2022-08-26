namespace VT2AssetLib.Stingray.Resources;

public enum BundleVersion : uint
{
    /// <summary>
    /// The 'base' bundle version, used for Vermintide 1.
    /// </summary>
    VT1 = 0xF0000004,

    /// <summary>
    /// The 'flags' bundle version, used for Vermintide 2.
    /// </summary>
    VT2 = 0xF0000005,

    /// <summary>
    /// The 'size' bundle version, used for Vermintide 2.X.
    /// </summary>
    VT2X = 0xF0000006,
}