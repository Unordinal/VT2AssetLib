using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using VT2AssetLib.Stingray;
using VT2AssetLib.Stingray.Resources;
using GltfPrimitiveType = SharpGLTF.Schema2.PrimitiveType;

namespace VT2AssetLib.IO.Gltf;

public static partial class GltfFile
{
    private static readonly Matrix4x4 _upZ = new
    (
        1, 0, 0, 0,
        0, 0, 1, 0,
        0, 1, 0, 0,
        0, 0, 0, 1
    );

    internal static void ExportFromUnit(string outputPath, UnitResource unit)
    {
        ModelRoot model = ModelRoot.CreateModel();
        Scene scene = model.UseScene(0);
        model.DefaultScene = scene;

        var unitAdapter = new GltfUnitAdapter(model, unit);
        unitAdapter.CreateModel(scene.CreateNode);

        model.ApplyBasisTransform(_upZ);

        WriteSettings settings = new()
        {
            Validation = SharpGLTF.Validation.ValidationMode.Skip
        };

        model.SaveGLB(outputPath, settings);
    }

    private static void TestModel(ModelRoot model, Scene scene)
    {
        Vector3[] pos = new Vector3[] { new Vector3(0.0f), new Vector3(0.0f), new Vector3(0.0f) };

        var m1 = model.CreateMesh("Mesh01");
        var mat1 = model.CreateMaterial("Material01");
        var mat2 = model.CreateMaterial("Material02");

        var p1 = m1.CreatePrimitive().WithMaterial(mat1)
            .WithIndicesAutomatic(GltfPrimitiveType.TRIANGLES)
            .WithVertexAccessor("POSITION", pos);

        var p2 = m1.CreatePrimitive().WithMaterial(mat2)
            .WithIndicesAutomatic(GltfPrimitiveType.TRIANGLES)
            .WithVertexAccessor("POSITION", pos);

        var m2 = model.CreateMesh("Mesh02");
        m2.CreatePrimitive()
            .WithIndicesAutomatic(GltfPrimitiveType.TRIANGLES)
            .WithVertexAccessor("POSITION", pos);

        var rootNode = scene.CreateNode("Node01").WithMesh(m1);

        scene.CreateNode("Node02").WithMesh(m2);

        var m3 = model.CreateMesh("Mesh03");
        m3.CreatePrimitive()
            .WithIndicesAutomatic(GltfPrimitiveType.TRIANGLES)
            .WithVertexAccessor("POSITION", pos)
            .WithVertexAccessor(GetTestJointsAccessor())
            .WithVertexAccessor(GetTestWeightsAccessor());

        var node03 = scene.CreateNode("Node03_Skinned");
        node03.Mesh = m3;

        var s1 = model.CreateSkin("Skin01");
        var j1 = node03.CreateNode("Joint01");
        var j2 = node03.CreateNode("Joint02");
        var j3 = node03.CreateNode("Joint03");
        s1.BindJoints(j1, j2, j3);

        node03.Skin = s1;

        static MemoryAccessor GetTestJointsAccessor()
        {
            MemoryAccessInfo jointsInfo = new("JOINTS_0", 0, 3, 0, DimensionType.VEC4, EncodingType.UNSIGNED_BYTE);
            return new MemoryAccessor(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, jointsInfo);
        }

        static MemoryAccessor GetTestWeightsAccessor()
        {
            MemoryAccessInfo weightsInfo = new("WEIGHTS_0", 0, 3, 0, DimensionType.VEC4, EncodingType.FLOAT);

            float[] weights = new float[] { 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, };
            return new MemoryAccessor(MemoryMarshal.AsBytes<float>(weights).ToArray(), weightsInfo);
        }
    }
}