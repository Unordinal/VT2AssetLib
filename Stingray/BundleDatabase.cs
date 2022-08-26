namespace VT2AssetLib.Stingray;

internal class BundleDatabase
{
    public uint Version { get; set; }

    public BundleFile[] BundleFiles { get; set; } = null!;

    public IDString64[]? ResourceHashes { get; set; } // Version >= 5

    internal class BundleFile
    {
        public uint Version { get; set; }

        public string Name { get; set; } = null!;

        public string StreamName { get; set; } = null!;

        public bool IsPlatformSpecific { get; set; }

        public byte[] Unk_20Bytes { get; set; } = null!;

        public DateTime FileTime { get; set; }
    }
}