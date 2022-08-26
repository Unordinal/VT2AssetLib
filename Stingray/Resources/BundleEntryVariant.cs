namespace VT2AssetLib.Stingray.Resources;

public sealed class BundleEntryVariant
{
    public ResourceLanguage Language { get; set; }

    public uint Size { get; set; }

    public uint StreamSize { get; set; }

    public byte[] Data { get; set; } = null!;
}