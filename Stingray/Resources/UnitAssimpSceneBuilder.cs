using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using VT2AssetLib.Extensions;
using VT2AssetLib.Numerics;
using VT2AssetLib.Stingray.Resources.Scene;

namespace VT2AssetLib.Stingray.Resources;

// TODO: Rework at some point.
internal static class UnitAssimpSceneBuilder
{
    private static readonly Assimp.Matrix4x4 _upZ = new
    (
        1, 0, 0, 0,
        0, 0, 1, 0,
        0, 1, 0, 0,
        0, 0, 0, 1
    );

    private static void AddMeshDataToNodeTree(UnitResource unitResource, Assimp.Node[] nodeTree, Dictionary<uint, int[]> meshMap)
    {
        foreach (var mesh in unitResource.Meshes)
        {
            if (!unitResource.HasGeometry(mesh))
                continue;

            var aiNode = nodeTree[mesh.NodeIndex];
            aiNode.MeshIndices.AddRange(meshMap[mesh.GeometryIndex]);
        }
    }

    private static Assimp.Node[] AddNodesToScene(Assimp.Scene scene, UnitResource unitResource)
    {
        var sceneGraph = unitResource.SceneGraph;
        var nodeTree = new Assimp.Node[sceneGraph.Nodes.Length];

        scene.RootNode = new Assimp.Node("Assimp Root Node");
        scene.RootNode.Transform *= _upZ;

        for (int i = 0; i < nodeTree.Length; i++)
        {
            var node = sceneGraph.Nodes[i];
            var nodeName = node.Name.ToString();

            var parent = node.ParentType != ParentType.None
                ? nodeTree[node.ParentIndex]
                : scene.RootNode;

            var aiNode = new Assimp.Node(nodeName, parent);
            parent.Children.Add(aiNode);
            aiNode.Transform = node.LocalTransform.ToAssimp();

            nodeTree[i] = aiNode;
        }

        return nodeTree;
    }

    /// <summary>
    /// Adds the materials in the unit resource to the Assimp scene.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="unitResource"></param>
    /// <returns>A material map that takes in the name of a material and returns the index of that material within the Assimp scene.</returns>
    private static Dictionary<IDString32, int> AddMaterialsToScene(Assimp.Scene scene, UnitResource unitResource)
    {
        var materialMap = new Dictionary<IDString32, int>();

        foreach (var mesh in unitResource.Meshes)
        {
            if (mesh.GeometryIndex <= 0) // No geometry exists.
                continue;

            var meshGeo = unitResource.MeshGeometries[mesh.GeometryIndex - 1];

            foreach (var batchRange in meshGeo.BatchRanges)
            {
                var mat = meshGeo.Materials[batchRange.MaterialIndex];
                if (materialMap.ContainsKey(mat))
                    continue;

                var aiMat = CreateAssimpMaterial(mat);
                materialMap[mat] = scene.MaterialCount;
                scene.Materials.Add(aiMat);
            }
        }

        return materialMap;
    }

    /// <summary>
    /// Adds the meshes in the unit resource to the Assimp scene.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="unitResource"></param>
    /// <returns>A mesh map that takes in the index of geometry and returns the indices of the matching meshes in the Assimp scene.</returns>
    private static Dictionary<uint, int[]> AddMeshesToScene(Assimp.Scene scene, UnitResource unitResource, Dictionary<IDString32, int> materialMap, bool createBones)
    {
        var meshesMap = new Dictionary<uint, int[]>();

        foreach (var mesh in unitResource.Meshes)
        {
            if (!unitResource.HasGeometry(mesh))
                continue;

            var meshGeo = unitResource.MeshGeometries[mesh.GeometryIndex - 1];
            var indexBuffer = meshGeo.IndexBuffer;
            var aiIndexBuffer = CreateAssimpIndexBuffer(indexBuffer);

            meshesMap[mesh.GeometryIndex] = new int[meshGeo.BatchRanges.Length];
            for (int i = 0; i < meshGeo.BatchRanges.Length; i++)
            {
                var meshIndices = meshesMap[mesh.GeometryIndex];
                var aiMesh = CreateAssimpMesh(unitResource, mesh, i, aiIndexBuffer, materialMap, createBones);
                aiMesh.Name = mesh.Name.ToString();

                meshIndices[i] = scene.MeshCount;
                scene.Meshes.Add(aiMesh);
            }
        }

        return meshesMap;
    }

    private static Assimp.Material CreateAssimpMaterial(IDString32 unitMaterial)
    {
        return new Assimp.Material
        {
            Name = unitMaterial.ToString(),
            ColorDiffuse = new Assimp.Color4D(0.8f),
            Shininess = 0.5f,
        };
    }

    // TODO: This method deals with creating the skeleton too, which is a problem when a model has no mesh but has a skeleton.
    // Find out the best way to resolve that.
    private static Assimp.Mesh CreateAssimpMesh(UnitResource unitResource, MeshObject meshObject, int batchRangeIndex, int[] aiIndexBuffer, Dictionary<IDString32, int> materialMap, bool createBones = true)
    {
        var meshGeometry = unitResource.GetGeometry(meshObject);
        var batchRange = meshGeometry.BatchRanges[batchRangeIndex];

        var aiMesh = new Assimp.Mesh(Assimp.PrimitiveType.Triangle);
        SetMeshVertexBuffers(aiMesh, meshGeometry.VertexBuffers);
        if (createBones)
            SetMeshBones(aiMesh, unitResource, meshObject, batchRangeIndex);

        const int IndicesPerFace = 3;
        var trisStart = (int)batchRange.Start * IndicesPerFace;
        var trisCount = (int)batchRange.Size * IndicesPerFace;

        bool indicesSuccess = aiMesh.SetIndices(aiIndexBuffer.AsSpan(trisStart, trisCount).ToArray(), IndicesPerFace);
        Debug.Assert(indicesSuccess, "indicesSuccess fail");

        var materialName = meshGeometry.Materials[batchRange.MaterialIndex];
        aiMesh.MaterialIndex = materialMap[materialName];
        return aiMesh;
    }

    private static void SetMeshVertexBuffers(Assimp.Mesh aiMesh, VertexBuffer[] vertexBuffers)
    {
        foreach (var vb in vertexBuffers)
        {
            var component = vb.Channel.Component;
            var set = vb.Channel.Set;
            switch (component)
            {
                case VertexComponent.Position:
                    var positions = GetPositionVectors(vb);
                    Debug.Assert(aiMesh.VertexCount == 0);
                    aiMesh.Vertices.AddRange(positions);
                    break;

                case VertexComponent.Normal:
                    var normals = GetNormalVectors(vb);
                    Debug.Assert(aiMesh.Normals.Count == 0);
                    aiMesh.Normals.AddRange(normals);
                    break;

                case VertexComponent.Tangent:
                    var tangents = GetTangentVectors(vb);
                    Debug.Assert(aiMesh.Tangents.Count == 0);
                    aiMesh.Tangents.AddRange(tangents);
                    break;

                case VertexComponent.Binormal:
                    var bitangents = GetBitangentVectors(vb);
                    Debug.Assert(aiMesh.BiTangents.Count == 0);
                    aiMesh.BiTangents.AddRange(bitangents);
                    break;

                case VertexComponent.Texcoord:
                    var texCoords = GetTexCoordVectors(vb);
                    var aiTexCoordsList = aiMesh.TextureCoordinateChannels[set];
                    Debug.Assert(aiTexCoordsList.Count == 0);
                    aiTexCoordsList.AddRange(texCoords);
                    aiMesh.UVComponentCount[set] = 2;
                    break;

                case VertexComponent.Color:
                    var colors = GetVertexColors(vb);
                    var aiColorsList = aiMesh.VertexColorChannels[set];
                    Debug.Assert(aiColorsList.Count == 0);
                    aiColorsList.AddRange(colors);
                    break;

                case VertexComponent.BlendIndices: // Add later.
                    continue;
                case VertexComponent.BlendWeights: // Ditto.
                    continue;
                default:
                    throw new InvalidOperationException($"Unsupported vb component type '{component}'");
            }
        }
    }

    private static void SetMeshBones(Assimp.Mesh aiMesh, UnitResource unitResource, MeshObject meshObject, int batchRangeIndex)
    {
        if (!unitResource.HasGeometry(meshObject))
            return;
        if (!unitResource.HasSkin(meshObject))
            return;

        var meshGeometry = unitResource.GetGeometry(meshObject);
        var meshSkin = unitResource.GetSkin(meshObject);

        var vertexBuffers = meshGeometry.VertexBuffers;
        var indicesBuffer = vertexBuffers.FirstOrDefault(vb => vb.Channel.Component == VertexComponent.BlendIndices);
        var weightsBuffer = vertexBuffers.FirstOrDefault(vb => vb.Channel.Component == VertexComponent.BlendWeights);

        Debug.Assert(indicesBuffer is not null && weightsBuffer is not null, "No vertex skinning data but mesh object has skin data!");

        var vecIndices = MemoryMarshal.Cast<byte, Vector4Byte>(indicesBuffer.Data);
        var vecWeights = MemoryMarshal.Cast<byte, Vector4Half>(weightsBuffer.Data);

        var batchRange = meshGeometry.BatchRanges[batchRangeIndex];
        var jointsData = meshSkin.GetJointsBelongingToSet(batchRange.BoneSet).ToArray();

        var boneMap = new Dictionary<byte, Assimp.Bone>(jointsData.Length);

        for (int i = 0; i < indicesBuffer.Count; i++)
        {
            var indices = vecIndices[i];
            var weights = vecWeights[i];

            AddBoneWeights(i, indices, weights);

            void AddBoneWeights(int vertexIndex, Vector4Byte boneIndices, Vector4Half boneWeights)
            {
                Vector4 vec4Weights = NormalizeSum(boneWeights);
                var existingBoneIndices = new HashSet<byte>();

                AddBoneWeight(boneIndices.X, vec4Weights.X);
                AddBoneWeight(boneIndices.Y, vec4Weights.Y);
                AddBoneWeight(boneIndices.Z, vec4Weights.Z);
                AddBoneWeight(boneIndices.W, vec4Weights.W);

                void AddBoneWeight(byte boneIndex, float boneWeight)
                {
                    if (!existingBoneIndices.Add(boneIndex))
                        return;

                    if (boneIndex >= jointsData.Length)
                        return;

                    if (!boneMap.TryGetValue(boneIndex, out var bone))
                    {
                        var (nodeIndex, invBindMatrix) = jointsData[boneIndex];
                        var node = unitResource.SceneGraph.Nodes[nodeIndex];

                        bone = new Assimp.Bone()
                        {
                            Name = node.Name.ToString(),
                            OffsetMatrix = invBindMatrix.ToAssimp(),
                        };

                        aiMesh.Bones.Add(bone);
                        boneMap[boneIndex] = bone;
                    }

                    bone.VertexWeights.Add(new Assimp.VertexWeight(i, boneWeight));
                }
            }
        }

        static Vector4 NormalizeSum(Vector4 vector)
        {
            return vector / (vector.X + vector.Y + vector.Z + vector.W);
        }
    }

    private static int[] CreateAssimpIndexBuffer(IndexBuffer indexBuffer)
    {
        int[] aiIndexBuffer = new int[indexBuffer.IndexCount];

        switch (indexBuffer.IndexFormat)
        {
            case IndexFormat.Index16:
                var indexShorts = MemoryMarshal.Cast<byte, ushort>(indexBuffer.Data);
                ConvertCopy<ushort, int>(indexShorts, aiIndexBuffer, v => v);
                break;

            case IndexFormat.Index32:
                var indexInts = MemoryMarshal.Cast<byte, int>(indexBuffer.Data);
                indexInts.CopyTo(aiIndexBuffer);
                break;

            default:
                throw new InvalidOperationException($"Invalid index format '{indexBuffer.IndexFormat}'");
        }

        return aiIndexBuffer;
    }

    private static Assimp.Vector3D[] GetPositionVectors(VertexBuffer positions)
    {
        Assimp.Vector3D[] aiData = new Assimp.Vector3D[positions.Count];
        Span<byte> rawData = positions.Data;

        var channelType = positions.Channel.Type;
        switch (channelType)
        {
            case ChannelType.Half4:
                var castData = MemoryMarshal.Cast<byte, Vector4Half>(rawData);
                ConvertCopy<Vector4Half, Assimp.Vector3D>(castData, aiData, (vec) => vec.ToAssimp());
                break;

            default:
                throw new InvalidOperationException($"Unsupported channel type '{channelType}' for {nameof(positions)}");
        }

        return aiData;
    }

    private static Assimp.Vector3D[] GetNormalVectors(VertexBuffer normals)
    {
        Assimp.Vector3D[] aiData = new Assimp.Vector3D[normals.Count];
        Span<byte> rawData = normals.Data;

        var channelType = normals.Channel.Type;
        switch (channelType)
        {
            case ChannelType.Half4:
                var castData = MemoryMarshal.Cast<byte, Vector4Half>(rawData);
                ConvertCopy<Vector4Half, Assimp.Vector3D>(castData, aiData, (vec) => vec.ToAssimp());
                break;

            default:
                throw new InvalidOperationException($"Unsupported channel type '{channelType}' for {nameof(normals)}");
        }

        return aiData;
    }

    private static Assimp.Vector3D[] GetTangentVectors(VertexBuffer tangents)
    {
        Assimp.Vector3D[] aiData = new Assimp.Vector3D[tangents.Count];
        Span<byte> rawData = tangents.Data;

        var channelType = tangents.Channel.Type;
        switch (channelType)
        {
            case ChannelType.Half4:
                var castData = MemoryMarshal.Cast<byte, Vector4Half>(rawData);
                ConvertCopy<Vector4Half, Assimp.Vector3D>(castData, aiData, (vec) => vec.ToAssimp());
                break;

            default:
                throw new InvalidOperationException($"Unsupported channel type '{channelType}' for {nameof(tangents)}");
        }

        return aiData;
    }

    private static Assimp.Vector3D[] GetBitangentVectors(VertexBuffer bitangents)
    {
        Assimp.Vector3D[] aiData = new Assimp.Vector3D[bitangents.Count];
        Span<byte> rawData = bitangents.Data;

        var channelType = bitangents.Channel.Type;
        switch (channelType)
        {
            case ChannelType.Half4:
                var castData = MemoryMarshal.Cast<byte, Vector4Half>(rawData);
                ConvertCopy<Vector4Half, Assimp.Vector3D>(castData, aiData, (vec) => vec.ToAssimp());
                break;

            default:
                throw new InvalidOperationException($"Unsupported channel type '{channelType}' for {nameof(bitangents)}");
        }

        return aiData;
    }

    private static Assimp.Vector3D[] GetTexCoordVectors(VertexBuffer texCoords)
    {
        Assimp.Vector3D[] aiData = new Assimp.Vector3D[texCoords.Count];
        Span<byte> rawData = texCoords.Data;

        var channelType = texCoords.Channel.Type;
        switch (channelType)
        {
            case ChannelType.Half2:
                var castData = MemoryMarshal.Cast<byte, Vector2Half>(rawData);
                ConvertCopy<Vector2Half, Assimp.Vector3D>(castData, aiData, (vec) => new Assimp.Vector3D(vec.ToAssimp(), 0f));
                break;

            default:
                throw new InvalidOperationException($"Unsupported channel type '{channelType}' for {nameof(texCoords)}");
        }

        return aiData;
    }

    private static Assimp.Color4D[] GetVertexColors(VertexBuffer colors)
    {
        var aiData = new Assimp.Color4D[colors.Count];
        var rawData = colors.Data;

        var channelType = colors.Channel.Type;
        switch (channelType)
        {
            case ChannelType.UByte4_NORM:
                var vecByteUnorms = MemoryMarshal.Cast<byte, Vector4Byte>(rawData);
                ConvertCopy<Vector4Byte, Assimp.Color4D>(vecByteUnorms, aiData, (v) =>
                {
                    var r = NormConversions.UnormToFloat(v.X);
                    var g = NormConversions.UnormToFloat(v.Y);
                    var b = NormConversions.UnormToFloat(v.Z);
                    var a = NormConversions.UnormToFloat(v.W);

                    return new Assimp.Color4D(r, g, b, a);
                });
                break;

            case ChannelType.Half4:
                var vec4Halfs = MemoryMarshal.Cast<byte, Vector4Half>(rawData);
                ConvertCopy<Vector4Half, Assimp.Color4D>(vec4Halfs, aiData, (v) => new Assimp.Color4D((float)v.X, (float)v.Y, (float)v.Z, (float)v.W));
                break;

            case ChannelType.Float3:
                var vec3s = MemoryMarshal.Cast<byte, Vector3>(rawData);
                ConvertCopy<Vector3, Assimp.Color4D>(vec3s, aiData, (v) => new Assimp.Color4D(v.X, v.Y, v.Z, 1.0f));
                break;

            default:
                throw new InvalidOperationException($"Unsupported channel type '{channelType}' for {nameof(colors)}");
        }

        return aiData;
    }

    private static Assimp.VertexWeight[] GetVertexWeights(VertexBuffer blendIndices, VertexBuffer blendWeights)
    {
        throw new NotImplementedException();
        Debug.Assert(blendIndices.Channel.Type == ChannelType.UByte4);
        Debug.Assert(blendWeights.Channel.Type == ChannelType.Half4);

        var indices = blendIndices.Data;
        var weights = MemoryMarshal.Cast<byte, Half>(blendWeights.Data);
        Debug.Assert(indices.Length == weights.Length);

        var vertexWeights = new Assimp.VertexWeight[indices.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            var index = indices[i];
            var weight = (float)weights[i];
            vertexWeights[i] = new Assimp.VertexWeight();
        }
    }

    private static void ConvertCopy<TFrom, TTo>(ReadOnlySpan<TFrom> source, Span<TTo> destination, Func<TFrom, TTo> converter)
    {
        Debug.Assert(source.Length == destination.Length);
        for (int i = 0; i < source.Length; i++)
        {
            destination[i] = converter(source[i]);
        }
    }

    public static Assimp.Scene Build(UnitResource unitResource)
    {
        var scene = new Assimp.Scene();
        scene.Metadata["UnitScaleFactor"] = new Assimp.Metadata.Entry(Assimp.MetaDataType.Float, 100.0f);
        //scene.Metadata["OriginalUnitScaleFactor"] = new Assimp.Metadata.Entry(Assimp.MetaDataType.Float, 1.0f);

        var nodeTree = AddNodesToScene(scene, unitResource);
        var materialMap = AddMaterialsToScene(scene, unitResource);
        var meshMap = AddMeshesToScene(scene, unitResource, materialMap, true);
        AddMeshDataToNodeTree(unitResource, nodeTree, meshMap);

        return scene;
    }
}