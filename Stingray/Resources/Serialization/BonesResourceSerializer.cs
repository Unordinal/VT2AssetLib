using System.Diagnostics;
using VT2AssetLib.IO;
using VT2AssetLib.Serialization;
using VT2AssetLib.Stingray.Serialization;

namespace VT2AssetLib.Stingray.Resources.Serialization;

internal sealed class BonesResourceSerializer : SerializerBase<BonesResource>
{
    public static BonesResourceSerializer Default { get; } = new(null);

    public IDString32Serializer IDString32Serializer { get; }

    public BonesResourceSerializer(IDString32Serializer? idString32Serializer)
    {
        IDString32Serializer = idString32Serializer ?? IDString32Serializer.Default;
    }

    protected override void SerializeValue(PrimitiveWriter writer, BonesResource value)
    {
        throw new NotImplementedException();
    }

    protected override void DeserializeValue(PrimitiveReader reader, out BonesResource result)
    {
        result = new BonesResource();

        uint boneCount = reader.ReadUInt32();
        uint lodCount = reader.ReadUInt32();

        result.BoneNameHashes = ArrayUtil.Create<IDString32>(boneCount);
        for (int i = 0; i < boneCount; i++)
            result.BoneNameHashes[i] = reader.Deserialize(IDString32Serializer);

        result.LodLevels = ArrayUtil.Create<uint>(lodCount);
        for (int i = 0; i < lodCount; i++)
            result.LodLevels[i] = reader.ReadUInt32();

        result.BoneNames = ArrayUtil.Create<string>(boneCount);
        for (int i = 0; i < boneCount; i++)
        {
            result.BoneNames[i] = reader.ReadCString();
            Debug.Assert(result.BoneNameHashes[i].ID == Murmur.Hash32(result.BoneNames[i]), "Bone name hash != Murmur.Hash32 name hash");
        }
    }
}