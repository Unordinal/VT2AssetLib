using System.Numerics;
using VT2AssetLib.IO;
using VT2AssetLib.Serialization;

namespace VT2AssetLib.Stingray.Serialization;

public class BoundingVolumeSerializer : SerializerBase<BoundingVolume>
{
    public static BoundingVolumeSerializer Default { get; } = new();

    protected override void SerializeValue(PrimitiveWriter writer, BoundingVolume value)
    {
        writer.Write(value.LowerBounds);
        writer.Write(value.UpperBounds);
        writer.Write(value.Radius);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out BoundingVolume result)
    {
        Vector3 lowerBounds = reader.ReadVector3();
        Vector3 upperBounds = reader.ReadVector3();
        float radius = reader.ReadSingle();

        result = new BoundingVolume(lowerBounds, upperBounds, radius);
    }
}