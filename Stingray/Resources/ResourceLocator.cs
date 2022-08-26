namespace VT2AssetLib.Stingray.Resources;

public readonly struct ResourceLocator : IEquatable<ResourceLocator>
{
    public IDString64 Type { get; }

    public IDString64 Name { get; }

    public ResourceLocator(IDString64 type, IDString64 name)
    {
        Type = type;
        Name = name;
    }

    public override bool Equals(object? obj)
    {
        return obj is ResourceLocator locator && Equals(locator);
    }

    public bool Equals(ResourceLocator other)
    {
        return Type == other.Type && Name == other.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Name);
    }

    public override string ToString()
    {
        return ToFilePath();
    }

    public string ToFilePath(ResourceLanguage language = ResourceLanguage.English)
    {
        const string FormatEng = "{0}.{1}";
        const string FormatOther = "{0}.{2}.{1}";

        return language == ResourceLanguage.English 
            ? string.Format(FormatEng, Name, Type) 
            : string.Format(FormatOther, Name, Type, language);
    }

    public static bool operator ==(ResourceLocator left, ResourceLocator right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ResourceLocator left, ResourceLocator right)
    {
        return !(left == right);
    }
}