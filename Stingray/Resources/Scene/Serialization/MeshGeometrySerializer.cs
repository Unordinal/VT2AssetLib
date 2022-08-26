using VT2AssetLib.IO;
using VT2AssetLib.Serialization;
using VT2AssetLib.Stingray.Resources.Scene;
using VT2AssetLib.Stingray.Serialization;

namespace VT2AssetLib.Stingray.Resources.Scene.Serialization;

internal sealed class MeshGeometrySerializer : SerializerBase<MeshGeometry>
{
    public static MeshGeometrySerializer Default { get; } = new(null);

    public IDString32Serializer IDString32Serializer { get; }

    private ArraySerializer<BatchRange> BatchRangesSerializer { get; }

    private ArraySerializer<IDString32> MaterialsSerializer { get; }

    public MeshGeometrySerializer(IDString32Serializer? idString32Serializer)
    {
        IDString32Serializer = idString32Serializer ?? IDString32Serializer.Default;
        BatchRangesSerializer = new ArraySerializer<BatchRange>(BatchRangeSerializer.Default);
        MaterialsSerializer = new ArraySerializer<IDString32>(IDString32Serializer);
    }

    protected override void SerializeValue(PrimitiveWriter writer, MeshGeometry value)
    {
        writer.Serialize(VertexBuffersSerializer.Default, value.VertexBuffers);
        writer.Serialize(IndexBufferSerializer.Default, value.IndexBuffer);
        writer.Serialize(BatchRangesSerializer, value.BatchRanges);
        writer.Serialize(BoundingVolumeSerializer.Default, value.BoundingVolume);
        writer.Serialize(MaterialsSerializer, value.Materials);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out MeshGeometry result)
    {
        result = new MeshGeometry
        {
            VertexBuffers = reader.Deserialize(VertexBuffersSerializer.Default),
            IndexBuffer = reader.Deserialize(IndexBufferSerializer.Default),
            BatchRanges = reader.Deserialize(BatchRangesSerializer),
            BoundingVolume = reader.Deserialize(BoundingVolumeSerializer.Default),
            Materials = reader.Deserialize(MaterialsSerializer),
        };
    }
}