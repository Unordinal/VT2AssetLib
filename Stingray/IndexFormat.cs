namespace VT2AssetLib.Stingray;

public enum IndexFormat : uint
{
    Index16,
    Index32,
}

public static class IndexFormatExtensions
{
    public static int GetSize(this IndexFormat indexFormat)
    {
        return indexFormat switch
        {
            IndexFormat.Index16 => 2,
            IndexFormat.Index32 => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(indexFormat), $"'{indexFormat}' is not a valid index format.")
        };
    }
}