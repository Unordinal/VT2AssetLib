using System.Numerics;
using VT2AssetLib.IO;
using VT2AssetLib.Serialization;
using VT2AssetLib.Stingray.Resources.Scene;

namespace VT2AssetLib.Stingray.Resources.Scene.Serialization;

internal sealed class SkinDataSerializer : SerializerBase<SkinData>
{
    public static SkinDataSerializer Default { get; } = new();

    private SkinDataSerializer()
    {
    }

    protected override void SerializeValue(PrimitiveWriter writer, SkinData value)
    {
        throw new NotImplementedException();
    }

    protected override void DeserializeValue(PrimitiveReader reader, out SkinData result)
    {
        result = new SkinData();

        uint ibmCount = reader.ReadUInt32();
        result.InvBindMatrices = ArrayUtil.Create<Matrix4x4>(ibmCount);

        for (int i = 0; i < ibmCount; i++)
            result.InvBindMatrices[i] = reader.ReadMatrix4x4();

        uint nodeIndexCount = reader.ReadUInt32();
        result.NodeIndices = ArrayUtil.Create<uint>(nodeIndexCount);

        for (int i = 0; i < nodeIndexCount; i++)
            result.NodeIndices[i] = reader.ReadUInt32();

        uint matrixIndexSetCount = reader.ReadUInt32();
        result.MatrixIndexSets = ArrayUtil.Create<uint[]>(matrixIndexSetCount);

        for (int i = 0; i < matrixIndexSetCount; i++)
        {
            uint setIndexCount = reader.ReadUInt32();
            result.MatrixIndexSets[i] = ArrayUtil.Create<uint>(setIndexCount);

            for (int j = 0; j < setIndexCount; j++)
                result.MatrixIndexSets[i][j] = reader.ReadUInt32();
        }
    }
}