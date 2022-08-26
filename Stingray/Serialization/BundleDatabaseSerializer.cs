using VT2AssetLib.IO;
using VT2AssetLib.Serialization;

namespace VT2AssetLib.Stingray.Serialization;

// TODO: Test.
internal class BundleDatabaseSerializer : SerializerBase<BundleDatabase>
{
    public static BundleDatabaseSerializer Default { get; } = new(null);

    public IDString64Serializer IDString64Serializer { get; }

    public BundleDatabaseSerializer(IDString64Serializer? idString64Serializer)
    {
        IDString64Serializer = idString64Serializer ?? IDString64Serializer.Default;
    }

    protected override void SerializeValue(PrimitiveWriter writer, BundleDatabase value)
    {
        throw new NotImplementedException();
    }

    protected override void DeserializeValue(PrimitiveReader reader, out BundleDatabase result)
    {
        result = new BundleDatabase();
        result.Version = reader.ReadUInt32();

        int bundleCount = reader.ReadInt32();

        var bundleFiles = new List<BundleDatabase.BundleFile>(bundleCount);
        for (int i = 0; i < bundleCount; i++) // This is a hashmap but we don't care about the hashes so we'll treat it like a normal array.
        {
            ulong bundleHash = reader.ReadUInt64();
            int bundlesInBucket = reader.ReadInt32();
            for (int j = 0; j < bundlesInBucket; j++)
            {
                var bundleFile = new BundleDatabase.BundleFile();
                bundleFiles.Add(bundleFile);

                bundleFile.Version = reader.ReadUInt32();
                if (bundleFile.Version == 4)
                {
                    bundleFile.Name = reader.ReadPString32();
                    bundleFile.StreamName = reader.ReadPString32();
                    bundleFile.IsPlatformSpecific = reader.ReadByte() != 0;
                    bundleFile.Unk_20Bytes = reader.ReadBytes(20);
                    bundleFile.FileTime = DateTime.FromFileTime(reader.ReadInt64());
                }
            }
        }

        if (result.Version < 5)
            return;

        int hashCount = reader.ReadInt32();
        result.ResourceHashes = ArrayUtil.Create<IDString64>(hashCount);
        for (int i = 0; i < result.ResourceHashes.Length; i++)
        {
            ulong resourceHashHash = reader.ReadUInt64(); // TODO: Should probably be equal to the resource hash itself? No example data to examine.
            result.ResourceHashes[i] = reader.Deserialize(IDString64Serializer);
        }
    }
}