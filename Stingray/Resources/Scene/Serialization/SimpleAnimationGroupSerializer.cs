using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VT2AssetLib.IO;
using VT2AssetLib.Serialization;
using VT2AssetLib.Stingray.Serialization;

namespace VT2AssetLib.Stingray.Resources.Scene.Serialization;
internal sealed class SimpleAnimationGroupSerializer : SerializerBase<SimpleAnimationGroup>
{
    public static SimpleAnimationGroupSerializer Default { get; } = new(null);

    public IDString32Serializer IDString32Serializer { get; }

    private ArraySerializer<int> GroupDataSerializer { get; }

    public SimpleAnimationGroupSerializer(IDString32Serializer? idString32Serializer)
    {
        IDString32Serializer = idString32Serializer ?? IDString32Serializer.Default;

        var intSerializer = new DelegateSerializer<int>
        (
            serializer: (writer, value) => writer.Write(value),
            deserializer: (reader) => reader.ReadInt32()
        );
        GroupDataSerializer = new ArraySerializer<int>(intSerializer);
    }

    protected override void SerializeValue(PrimitiveWriter writer, SimpleAnimationGroup value)
    {
        writer.Serialize(IDString32Serializer, value.Name);
        writer.Serialize(GroupDataSerializer, value.Data);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out SimpleAnimationGroup result)
    {
        result = new SimpleAnimationGroup
        {
            Name = reader.Deserialize(IDString32Serializer),
            Data = reader.Deserialize(GroupDataSerializer)
        };
    }
}
