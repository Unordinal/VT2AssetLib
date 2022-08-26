using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using VT2AssetLib.Stingray;
using VT2AssetLib.Stingray.Resources;
using VT2AssetLib.Stingray.Resources.Scene;
using GltfPrimitiveType = SharpGLTF.Schema2.PrimitiveType;

namespace VT2AssetLib.IO.Gltf;

// TODO: There's a better way to do this; probably something like making a new class to hold a node and its VT2 data,
// so we can preprocess the data and then from there build a glTF scene tree out of it which would give us more freedom with the
// node tree layout than we currently have.
// TODO: This is kind of a mess; clean it up.
// TODO, NOTE: Currently using Assimp instead but Assimp.NET support is broken for glTF objects that deal with skinned data.
internal class GltfUnitAdapter
{
    private readonly ModelRoot _model;
    private readonly UnitResource _unit;
    private readonly SceneGraph _sceneGraph;
    private readonly MeshObject[] _meshObjects;
    private readonly MeshGeometry[] _meshGeometries;
    private readonly SkinData[] _skinDatas;
    private readonly IDString32[][] _materials;

    private readonly Dictionary<uint, Mesh[]> _gltfMeshes; // uint = GeometryIndex, Mesh index = BatchRange.BoneSet
    private readonly Dictionary<uint, Skin[]> _gltfSkins; // uint = GeometryIndex, Skin index = BatchRange.BoneSet
    private readonly Dictionary<IDString32, Material> _gltfMaterials;

    public GltfUnitAdapter(ModelRoot model!!, UnitResource unit!!)
    {
        _model = model;
        _unit = unit;
        _sceneGraph = unit.SceneGraph;
        _meshObjects = unit.Meshes;
        _meshGeometries = unit.MeshGeometries;
        _skinDatas = unit.Skins;
        _materials = _meshGeometries.Select(geo => geo.Materials).ToArray();

        _gltfMeshes = new(_meshObjects.Length);
        _gltfSkins = new(_skinDatas.Length);
        _gltfMaterials = new(_materials.Length);
    }

    public void CreateModel(Func<string, Node> rootNodeCreator)
    {
        var nodeTree = CreateNodeTree(rootNodeCreator);
        GenerateGltfData(nodeTree);

        for (int i = 0; i < _meshObjects.Length; i++)
        {
            var meshObject = _meshObjects[i];
            if (!HasMeshGeo(meshObject))
                continue;

            var gltfNode = nodeTree[meshObject.NodeIndex];

            var gltfMeshes = _gltfMeshes[meshObject.GeometryIndex];
            var gltfSkins = _gltfSkins[meshObject.GeometryIndex];
            for (int j = 0; j < gltfMeshes.Length; j++)
            {
                var gltfMesh = gltfMeshes[j];
                var gltfSkin = gltfSkins[j];

                Node geoNode = gltfNode;
                if (gltfSkin is not null)
                {
                    geoNode = gltfNode.CreateNode(gltfSkin.Name);
                    geoNode.Skin = gltfSkin;
                }

                geoNode.Mesh = gltfMesh;
            }
        }
    }

    private Node[] CreateNodeTree(Func<string, Node> rootNodeCreator)
    {
        Debug.Assert(_sceneGraph.Nodes is not null);

        Node[] nodeTree = new Node[_sceneGraph.Nodes.Length];
        for (int i = 0; i < _sceneGraph.Nodes.Length; i++)
        {
            var node = _sceneGraph.Nodes[i];
            var nodeName = node.Name.ToString();

            nodeTree[i] = node.ParentType != ParentType.None
                ? nodeTree[node.ParentIndex].CreateNode(nodeName)
                : rootNodeCreator(nodeName);

            nodeTree[i].LocalTransform = node.LocalTransform;
        }

        return nodeTree;
    }

    private void GenerateGltfData(Node[] nodeTree)
    {
        foreach (var material in _materials.SelectMany(m => m))
        {
            _gltfMaterials[material] = CreateBasicMaterial(material.ToString());
        }

        for (int i = 0; i < _meshObjects.Length; i++)
        {
            var meshObject = _meshObjects[i];

            if (!TryGetMeshGeo(meshObject, out var meshGeo))
                throw new ArgumentException($"The mesh object at index '{i}' did not have any valid geometry.");

            int boneSetCount = meshGeo.BatchRanges.Select(br => br.BoneSet).Distinct().Count();
            var meshes = _gltfMeshes[meshObject.GeometryIndex] = new Mesh[boneSetCount];
            var skins = _gltfSkins[meshObject.GeometryIndex] = new Skin[boneSetCount];

            for (int j = 0; j < boneSetCount; j++)
            {
                meshes[j] = CreateMeshForBoneSet(meshObject, j);
                if (HasSkinData(meshObject))
                    skins[j] = CreateSkinForBoneSet(meshObject, j, nodeTree);
            }

            /*for (int j = 0; j < meshGeo.BatchRanges.Length; j++)
            {
                var batchRange = meshGeo.BatchRanges[j];

                if (!_gltfMeshes.TryGetValue(meshObject.GeometryIndex, out var meshes))
                {
                    int boneSets = meshGeo.BatchRanges.Select(br => br.BoneSet).Distinct().Count();
                    meshes = _gltfMeshes[meshObject.GeometryIndex] = new Mesh[boneSets];
                }

                int boneSetIdx = (int)batchRange.BoneSet - 1;
                Debug.Assert(boneSetIdx >= 0);
                if (meshes[boneSetIdx] is null)
                    meshes[boneSetIdx] = CreateMeshForBoneSet(meshObject, batchRange.BoneSet);
            }*/
        }
    }

    private Mesh CreateMeshForBoneSet(MeshObject meshObject!!, int boneSetIndex)
    {
        Debug.Assert(boneSetIndex >= 0);
        if (!TryGetMeshGeo(meshObject, out var meshGeo))
            throw new ArgumentException($"The given mesh object did not have any valid geometry.", nameof(meshObject));

        var indexBuffer = meshGeo.IndexBuffer;
        var indexFormatSize = indexBuffer.IndexFormat.GetSize();
        var vertexBuffers = meshGeo.VertexBuffers;

        var buffersAdapter = new GltfBuffersAdapter(indexBuffer, vertexBuffers);
        var vertexAccessors = buffersAdapter.GetVertexAccessors();

        var mesh = _model.CreateMesh($"{meshObject.Name}{{{boneSetIndex}}}");

        foreach (var batchRange in meshGeo.BatchRanges.Where(br => br.BoneSet == boneSetIndex))
        {
            // Batch range indices are in triangle count.
            var triangleStride = indexFormatSize * 3; // A triangle is three indices and each index is two or four bytes long.
            var trisStart = (int)batchRange.Start * triangleStride;
            var trisCount = (int)batchRange.Size * triangleStride;

            var indicesAccessor = buffersAdapter.GetIndexAccessor(trisStart, trisCount);
            var material = GetGltfMaterial(meshObject, batchRange.MaterialIndex);

            mesh.CreatePrimitive()
                .WithVertexAccessors(vertexAccessors)
                .WithIndicesAccessor(GltfPrimitiveType.TRIANGLES, indicesAccessor)
                .WithMaterial(material);
        }

        return mesh;
    }

    private Skin CreateSkinForBoneSet(MeshObject meshObject!!, int boneSetIndex, Node[] nodeTree)
    {
        Debug.Assert(boneSetIndex >= 0);
        if (!TryGetMeshGeo(meshObject, out var meshGeo))
            throw new ArgumentException($"The given mesh object did not have any valid geometry.", nameof(meshObject));
        if (!TryGetSkinData(meshObject, out var skinData))
            throw new ArgumentException($"The given mesh object did not have any valid skin data.", nameof(meshObject));

        var joints = new List<(Node, Matrix4x4)>();
        foreach (var batchRange in meshGeo.BatchRanges.Where(br => br.BoneSet == boneSetIndex))
        {
            var jointsInSet = skinData
                .GetJointsBelongingToSet((uint)boneSetIndex)
                .Select(p => (nodeTree[p.NodeIndex], p.InvBindMatrix));

            joints.AddRange(jointsInSet);
        }

        for (int i = 0; i < joints.Count; i++)
        {
            (Node, Matrix4x4) joint = joints[i];
            var matr = joint.Item2;
            if (matr.M14 != 0f || matr.M24 != 0f || matr.M34 != 0f || matr.M44 != 1f)
            {
                Trace.WriteLine($"InvBindMatrix '{i}' in mesh object '{meshObject.Name}[{boneSetIndex}]' had non-zero fourth column, 'fixing'. ({matr})");
                matr.M14 = 0f;
                matr.M24 = 0f;
                matr.M34 = 0f;
                matr.M44 = 1f;

                joints[i] = (joint.Item1, matr);
            }

            if (!Matrix4x4.Invert(matr, out _))
                Trace.WriteLine($"InvBindMatrix '{i}' is not invertible. This will throw.");
        }

        var skin = _model.CreateSkin($"{meshObject.Name}{{{boneSetIndex}}} Armature");
        skin.BindJoints(joints.ToArray());

        return skin;
    }

    private bool HasMeshGeo(MeshObject meshObject!!)
    {
        return meshObject.GeometryIndex > 0;
    }

    private bool HasSkinData(MeshObject meshObject!!)
    {
        return meshObject.SkinIndex - _meshGeometries.Length > 0;
    }

    private bool TryGetMeshGeo(MeshObject meshObject!!, [NotNullWhen(true)] out MeshGeometry? meshGeometry)
    {
        if (meshObject.GeometryIndex > 0)
        {
            meshGeometry = _meshGeometries[meshObject.GeometryIndex - 1];
            return true;
        }

        meshGeometry = null;
        return false;
    }

    private bool TryGetSkinData(MeshObject meshObject!!, [NotNullWhen(true)] out SkinData? skinData)
    {
        if (meshObject.SkinIndex - _meshGeometries.Length > 0)
        {
            skinData = _skinDatas[meshObject.SkinIndex - _meshObjects.Length - 1];
            return true;
        }

        skinData = null;
        return false;
    }

    private Material GetGltfMaterial(MeshObject meshObject, uint materialIndex)
    {
        if (!TryGetMeshGeo(meshObject, out var meshGeo))
            throw new ArgumentException("The given mesh object does not have any valid geometry.");
        if (materialIndex >= meshGeo.Materials.Length)
            throw new ArgumentOutOfRangeException(nameof(materialIndex), $"The given mesh object does not have a material with index '{materialIndex}'.");

        var srMaterial = meshGeo.Materials[materialIndex];
        Debug.Assert(_gltfMaterials.ContainsKey(srMaterial), $"No glTF material for SR material '{srMaterial}' (Mesh '{meshObject.Name}', mat idx '{materialIndex}')");

        return _gltfMaterials[srMaterial];
    }

    private Material CreateBasicMaterial(string name!!)
    {
        var material = new MaterialBuilder(name)
                    .WithBaseColor(new Vector4(0.5f, 0.5f, 0.5f, 1.0f))
                    .WithMetallicRoughness(0.0f, 0.5f);

        material.IndexOfRefraction = 1.5f;
        return _model.CreateMaterial(material);
    }
}