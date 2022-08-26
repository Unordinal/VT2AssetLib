using System.Diagnostics;
using VT2AssetLib.IO;
using VT2AssetLib.Serialization;
using VT2AssetLib.Stingray.Resources.Scene;
using VT2AssetLib.Stingray.Resources.Scene.Serialization;
using VT2AssetLib.Stingray.Serialization;

namespace VT2AssetLib.Stingray.Resources.Serialization;

internal sealed class UnitResourceSerializer : SerializerBase<UnitResource>
{
    public IDString32Serializer IDString32Serializer { get; }

    private ArraySerializer<MeshGeometry> MeshGeometriesSerializer { get; }

    private ArraySerializer<SkinData> SkinDatasSerializer { get; }

    // TODO: It's more efficient to handle a primitive array manually but it doesn't make sense on first glance when ArraySerializer exists -
    // should probably resolve that. Alternatively, don't have an ArraySerializer at all and handle arrays manually at each point. That would
    // make it more explicit.
    private DelegateSerializer<byte[]> SimpleAnimationSerializer { get; } // Maybe I'm getting a little ridiculous with these...

    private ArraySerializer<SimpleAnimationGroup> SimpleAnimationGroupsSerializer { get; }

    private SceneGraphSerializer SceneGraphSerializer { get; }

    private ArraySerializer<MeshObject> MeshObjectsSerializer { get; }

    public UnitResourceSerializer(IDString32Serializer? idString32Serializer)
    {
        IDString32Serializer = idString32Serializer ?? IDString32Serializer.Default;
        MeshGeometriesSerializer = new ArraySerializer<MeshGeometry>(new MeshGeometrySerializer(IDString32Serializer));
        SkinDatasSerializer = new ArraySerializer<SkinData>(SkinDataSerializer.Default);
        SimpleAnimationSerializer = new DelegateSerializer<byte[]>
        (
            serializer: (writer, value) =>
            {
                writer.Write(value.Length);
                writer.Write(value);
            },
            deserializer: (reader) => reader.ReadBytes(reader.ReadInt32())
        );
        SimpleAnimationGroupsSerializer = new ArraySerializer<SimpleAnimationGroup>(new SimpleAnimationGroupSerializer(IDString32Serializer));
        SceneGraphSerializer = new SceneGraphSerializer(IDString32Serializer);
        MeshObjectsSerializer = new ArraySerializer<MeshObject>(new MeshObjectSerializer(IDString32Serializer));
    }

    protected override void SerializeValue(PrimitiveWriter writer, UnitResource value)
    {
        writer.Write(value.Version);
        writer.Serialize(MeshGeometriesSerializer, value.MeshGeometries);
        writer.Serialize(SkinDatasSerializer, value.Skins);
        writer.Serialize(SimpleAnimationSerializer, value.SimpleAnimation);
        writer.Serialize(SimpleAnimationGroupsSerializer, value.SimpleAnimationGroups);
        writer.Serialize(SceneGraphSerializer, value.SceneGraph);
        writer.Serialize(MeshObjectsSerializer, value.Meshes);
    }

    protected override void DeserializeValue(PrimitiveReader reader, out UnitResource result)
    {
        uint version = reader.ReadUInt32();
        if (version != 186)
            Trace.WriteLine($"Unsupported unit resource version '{version}' - there may be problems with reading the resource.");

        result = new UnitResource
        {
            Version = version,
            MeshGeometries = reader.Deserialize(MeshGeometriesSerializer),
            Skins = reader.Deserialize(SkinDatasSerializer),
            SimpleAnimation = reader.Deserialize(SimpleAnimationSerializer),
            SimpleAnimationGroups = reader.Deserialize(SimpleAnimationGroupsSerializer),
            SceneGraph = reader.Deserialize(SceneGraphSerializer),
            Meshes = reader.Deserialize(MeshObjectsSerializer),
        };
    }
}