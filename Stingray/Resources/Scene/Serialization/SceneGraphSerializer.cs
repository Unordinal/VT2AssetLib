using System.Numerics;
using VT2AssetLib.IO;
using VT2AssetLib.Numerics;
using VT2AssetLib.Serialization;
using VT2AssetLib.Stingray.Serialization;

namespace VT2AssetLib.Stingray.Resources.Scene.Serialization;

internal sealed class SceneGraphSerializer : SerializerBase<SceneGraph>
{
    public static SceneGraphSerializer Default { get; } = new(null);

    public IDString32Serializer IDString32Serializer { get; }

    public SceneGraphSerializer(IDString32Serializer? idString32Serializer)
    {
        IDString32Serializer = idString32Serializer ?? IDString32Serializer.Default;
    }

    protected override void SerializeValue(PrimitiveWriter writer, SceneGraph value)
    {
        throw new NotImplementedException();
    }

    protected override void DeserializeValue(PrimitiveReader reader, out SceneGraph result)
    {
        result = new SceneGraph();

        uint nodeCount = reader.ReadUInt32();
        result.Nodes = ArrayUtil.CreateAndPopulate<SceneGraphData>(nodeCount);

        for (int i = 0; i < nodeCount; i++)
        {
            var node = result.Nodes[i];

            Matrix4x4 rotation = reader.ReadMatrix3x3();
            Vector3 position = reader.ReadVector3();
            Vector3 scale = reader.ReadVector3();

            node.LocalTransform = MatrixUtil.GetTRSMatrix(position, rotation, scale);
        }

        foreach (var node in result.Nodes)
        {
            node.WorldTransform = reader.ReadMatrix4x4();
        }

        foreach (var node in result.Nodes)
        {
            node.ParentType = (ParentType)reader.ReadUInt16();
            node.ParentIndex = reader.ReadUInt16();
        }

        foreach (var node in result.Nodes)
        {
            node.Name = reader.Deserialize(IDString32Serializer);
        }
    }
}