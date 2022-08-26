using VT2AssetLib.IO;
using VT2AssetLib.Serialization;

namespace VT2AssetLib.Stingray.Serialization;

public class IDString64Serializer : SerializerBase<IDString64>
{
    public static IDString64Serializer Default { get; } = new(null);

    public IDStringRepository IDStringRepository { get; }

    public IDString64Serializer(IDStringRepository? idStringRepo)
    {
        IDStringRepository = idStringRepo ?? IDStringRepository.Shared;
    }

    protected override void SerializeValue(PrimitiveWriter writer, IDString64 value)
    {
        writer.Write(value.ID);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out IDString64 result)
    {
        ulong hash = reader.ReadUInt64();
        result = IDStringRepository.GetOrCreate(hash);
    }
}