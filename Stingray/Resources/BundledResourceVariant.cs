namespace VT2AssetLib.Stingray.Resources;

public sealed class BundledResourceVariant
{
    /// <summary>
    /// Gets the language this variant is in.
    /// </summary>
    public ResourceLanguage Language => _entryVariant.Language;

    /// <summary>
    /// Gets the raw data of this resource variant.
    /// </summary>
    public byte[] Data => _entryVariant.Data;

    private readonly BundleEntryVariant _entryVariant;

    internal BundledResourceVariant(BundleEntryVariant entryVariant)
    {
        ArgumentNullException.ThrowIfNull(entryVariant);
        _entryVariant = entryVariant;
    }
}