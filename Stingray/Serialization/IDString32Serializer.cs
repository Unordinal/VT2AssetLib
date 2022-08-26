using VT2AssetLib.IO;
using VT2AssetLib.Serialization;

namespace VT2AssetLib.Stingray.Serialization;

public class IDString32Serializer : SerializerBase<IDString32>
{
    public static IDString32Serializer Default { get; } = new(null);

    public IDStringRepository IDStringRepository { get; }

    public IDString32Serializer(IDStringRepository? idStringRepo)
    {
        IDStringRepository = idStringRepo ?? IDStringRepository.Shared;
    }

    protected override void SerializeValue(PrimitiveWriter writer, IDString32 value)
    {
        writer.Write(value.ID);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out IDString32 result)
    {
        uint hash = reader.ReadUInt32();
        result = IDStringRepository.GetOrCreate(hash);
    }
}