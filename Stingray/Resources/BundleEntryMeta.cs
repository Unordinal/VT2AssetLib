namespace VT2AssetLib.Stingray.Resources;

public sealed class BundleEntryMeta
{
    public ResourceLocator Locator { get; set; }

    public ResourceFlag? Flag { get; set; }

    public uint? Size { get; set; }

    public override string ToString()
    {
        string result = "";

        if (Flag is not null and not ResourceFlag.None)
            result += $"[{Flag}] ";

        result += Locator.ToString();

        if (Size is not null)
            result += $" ({Size} bytes)";

        return result;
    }

    public static int GetSizeForBundleVersion(BundleVersion bundleVersion)
    {
        return bundleVersion switch
        {
            BundleVersion.VT1 => 16,
            BundleVersion.VT2 => 20,
            BundleVersion.VT2X => 24,
            _ => throw new ArgumentOutOfRangeException(nameof(bundleVersion))
        };
    }
}