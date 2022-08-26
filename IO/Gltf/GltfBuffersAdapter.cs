//#define DEBUG_VERTEX_BUFFERS
using SharpGLTF.Memory;
using SharpGLTF.Schema2;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VT2AssetLib.Numerics;
using VT2AssetLib.Stingray;

namespace VT2AssetLib.IO.Gltf;

internal class GltfBuffersAdapter
{
    private readonly IndexBuffer _indexBuffer;
    private readonly VertexBuffer[] _vertexBuffers;

    public GltfBuffersAdapter(IndexBuffer indexBuffer!!, VertexBuffer[] vertexBuffers!!)
    {
        _indexBuffer = indexBuffer;
        _vertexBuffers = vertexBuffers;
    }

    public MemoryAccessor GetIndexAccessor(int triStart, int triCount)
    {
        int indexSize = _indexBuffer.IndexFormat.GetSize();
        EncodingType encodingType = indexSize == 4
            ? EncodingType.UNSIGNED_INT
            : EncodingType.UNSIGNED_SHORT;

        AttributeFormat format = new(DimensionType.SCALAR, encodingType);

        var indicesSegment = new ArraySegment<byte>(_indexBuffer.Data!, triStart, triCount);
        int indicesCount = triCount / indexSize;

        return GetAccessor("INDICES", indicesSegment, indicesCount, format);
    }

    public MemoryAccessor[] GetVertexAccessors()
    {
#if DEBUG_VERTEX_BUFFERS
        Debug.WriteLine($"Processing {_vertexBuffers.Length} vertex buffers...");
#endif

        List<MemoryAccessor> accessors = new(_vertexBuffers.Length);
        byte[]? bitangentsRaw = null;
        uint texcoordSet = 0;
        uint colorSet = 0;
        uint jointSet = 0;
        uint weightSet = 0;

        for (int i = 0; i < _vertexBuffers.Length; i++)
        {
            var vertexBuffer = _vertexBuffers[i];
            var data = vertexBuffer.Data;
            var component = vertexBuffer.Channel.Component;
            // TODO: var set = component.Set; <- do this instead for the set vars if I come back to this
            
            MemoryAccessor? accessor = null;
            switch (component)
            {
                case VertexComponent.Position:
                    accessor = GetPositionsAccessor(data);
                    break;

                case VertexComponent.Normal:
                    accessor = GetNormalsAccessor(data);
                    break;

                case VertexComponent.Tangent:
                    accessor = GetTangentsAccessor(data);
                    break;

                case VertexComponent.Binormal:
                    bitangentsRaw = ConvertBitangents(data);
                    break;

                case VertexComponent.Texcoord:
                    accessor = GetTexcoordsAccessor(data, texcoordSet++);
                    break;

                case VertexComponent.Color:
                    accessor = GetColorsAccessor(vertexBuffer, colorSet++);
                    break;

                case VertexComponent.BlendIndices:
                    accessor = GetBlendIndicesAccessor(data, jointSet++);
                    break;

                case VertexComponent.BlendWeights:
                    accessor = GetBlendWeightsAccessor(data, weightSet++);
                    break;

                default:
                    throw new NotSupportedException($"The specified vertex component is not supported: '{component}'");
            }

            if (accessor is not null)
                accessors.Add(accessor);
        }

        // glTF doesn't accept bitangents and our tangents don't have a handedness sign. Let's fix that by calculating it via cross-product.
        var normalsAcc = accessors.Find(ma => ma.Attribute.Name == "NORMAL");
        var tangentsAcc = accessors.Find(ma => ma.Attribute.Name == "TANGENT");
        if (normalsAcc != null && tangentsAcc != null && bitangentsRaw != null)
        {
            var normals = MemoryMarshal.Cast<byte, Vector3>(normalsAcc.Data);
            var tangents = MemoryMarshal.Cast<byte, Vector4>(tangentsAcc.Data);
            var bitangents = MemoryMarshal.Cast<byte, Vector4>(tangentsAcc.Data);

            CalculateTangentsW(tangents, bitangents, normals);
        }

#if DEBUG_VERTEX_BUFFERS
        // I kinda hate this.
        Dictionary<VertexComponent, string> componentToGltfName = new()
        {
            { VertexComponent.Position, "POSITION" },
            { VertexComponent.Normal, "NORMAL" },
            { VertexComponent.Tangent, "TANGENT" },
            { VertexComponent.Texcoord, "TEXCOORD_" },
            { VertexComponent.Color, "COLOR_" },
            { VertexComponent.BlendIndices, "JOINTS_" },
            { VertexComponent.BlendWeights, "WEIGHTS_" },
        };
        for (int i = 0; i < _vertexBuffers.Length; i++)
        {
            VertexBuffer vertexBuffer = _vertexBuffers[i];
            Debug.WriteLine($"\tVB ({vertexBuffer.Validity}, {vertexBuffer.StreamType}, {vertexBuffer.Channel}): [Elements: {vertexBuffer.Count}, Length: {vertexBuffer.Data!.Length}]");

            string? startsWith = componentToGltfName.GetValueOrDefault(vertexBuffer.Channel!.Component);
            if (startsWith is null)
                continue;

            MemoryAccessor? accessor = accessors.FirstOrDefault(ma => ma.Attribute.Name.StartsWith(startsWith));
            if (accessor is null)
                continue;

            Debug.WriteLine($"\tACC ({accessor.Attribute.Name}): [Elements: {accessor.Attribute.ItemsCount}, Length: {accessor.Data.Count}]");
            var vbItems = vertexBuffer.Count;
            var accItems = accessor.Attribute.ItemsCount;
            var comp = vertexBuffer.Channel.Component;
            var set = vertexBuffer.Channel.Set;
            Debug.Assert(vbItems == accItems, $"[{comp}_{set}]: Count mismatch ({vbItems} vs {accItems})");
        }
#endif

        return accessors.ToArray();
    }

    private static MemoryAccessor GetPositionsAccessor(byte[] positions)
    {
        int oldStride = sizeof(ushort) * 4; // sizeof(Half) * 4
        int newStride = sizeof(float) * 3;
        var result = GetArrayWithNewStride(positions.Length, oldStride, newStride);

        TransformSpan<Vector4Half, Vector3>(positions, result, (pos) => (Vector3)pos);

        int count = result.Length / newStride;
        var format = new AttributeFormat(DimensionType.VEC3, EncodingType.FLOAT);
        var accessor = GetAccessor("POSITION", result, count, format);
        return accessor;
    }

    private static MemoryAccessor GetNormalsAccessor(byte[] normals)
    {
        int oldStride = sizeof(ushort) * 4;
        int newStride = sizeof(float) * 3;
        var result = GetArrayWithNewStride(normals.Length, oldStride, newStride);

        TransformSpan<Vector4Half, Vector3>(normals, result, (norm) => (Vector3)norm);

        int count = result.Length / newStride;
        var format = new AttributeFormat(DimensionType.VEC3, EncodingType.FLOAT);
        var accessor = GetAccessor("NORMAL", result, count, format);
        return accessor;
    }

    private static MemoryAccessor GetTangentsAccessor(byte[] tangents)
    {
        int oldStride = sizeof(ushort) * 4;
        int newStride = sizeof(float) * 4;
        var result = GetArrayWithNewStride(tangents.Length, oldStride, newStride);

        TransformSpan<Vector4Half, Vector4>(tangents, result, (tang) => (Vector4)tang);

        int count = result.Length / newStride;
        var format = new AttributeFormat(DimensionType.VEC4, EncodingType.FLOAT);
        var accessor = GetAccessor("TANGENT", result, count, format);
        return accessor;
    }

    private static MemoryAccessor GetTexcoordsAccessor(byte[] texcoords, uint set)
    {
        int oldStride = sizeof(ushort) * 2;
        int newStride = sizeof(float) * 2;
        var result = GetArrayWithNewStride(texcoords.Length, oldStride, newStride);

        TransformSpan<Vector2Half, Vector2>(texcoords, result, (tang) => (Vector2)tang);

        int count = result.Length / newStride;
        var format = new AttributeFormat(DimensionType.VEC2, EncodingType.FLOAT);
        var accessor = GetAccessor($"TEXCOORD_{set}", result, count, format);
        return accessor;
    }

    private static MemoryAccessor GetColorsAccessor(VertexBuffer colors, uint set)
    {
        int oldStride = (int)colors.Stride;
        int newStride;
        byte[] result;

        AttributeFormat format;
        switch (colors.Channel.Type)
        {
            case ChannelType.UByte4_NORM:
                newStride = sizeof(byte) * 4;
                result = colors.Data;
                format = new AttributeFormat(DimensionType.VEC4, EncodingType.UNSIGNED_BYTE, true);
                break;
            case ChannelType.Float3:
                newStride = sizeof(float) * 3;
                result = colors.Data;
                format = new AttributeFormat(DimensionType.VEC3, EncodingType.FLOAT);
                break;
            case ChannelType.Half4:
                newStride = sizeof(float) * 4;
                result = GetArrayWithNewStride(colors.Data!.Length, oldStride, newStride);
                TransformSpan<Vector4Half, Vector4>(colors.Data, result, (color) => (Vector4)color);
                format = new AttributeFormat(DimensionType.VEC4, EncodingType.FLOAT);
                break;
            default:
                throw new NotSupportedException($"Unsupported 'COLOR' vertex buffer format: {colors.Channel.Type}");
        }

        int count = result.Length / newStride;
        var accessor = GetAccessor($"COLOR_{set}", result, count, format);
        return accessor;
    }

    private static MemoryAccessor GetBlendIndicesAccessor(byte[] blendIndices, uint set)
    {
        int stride = sizeof(byte) * 4;
        int count = blendIndices.Length / stride;
        var format = new AttributeFormat(DimensionType.VEC4, EncodingType.UNSIGNED_BYTE);
        var accessor = GetAccessor($"JOINTS_{set}", blendIndices, count, format);
        return accessor;
    }

    private static MemoryAccessor GetBlendWeightsAccessor(byte[] blendWeights, uint set)
    {
        int oldStride = sizeof(ushort) * 4;
        int newStride = sizeof(float) * 4;
        var result = GetArrayWithNewStride(blendWeights.Length, oldStride, newStride);

        TransformSpan<Vector4Half, Vector4>(blendWeights, result, (wgts) => NormalizeSum((Vector4)wgts));

        int count = result.Length / newStride;
        var format = new AttributeFormat(DimensionType.VEC4, EncodingType.FLOAT);
        var accessor = GetAccessor($"WEIGHTS_{set}", result, count, format);
        return accessor;
    }

    private static MemoryAccessor GetAccessor(string name, ArraySegment<byte> values, int elementCount, AttributeFormat format)
    {
        MemoryAccessInfo info = new(name, 0, elementCount, 0, format);

        return new MemoryAccessor(values, info);
    }

    private static byte[] ConvertBitangents(byte[] bitangents)
    {
        int oldStride = sizeof(ushort) * 4;
        int newStride = sizeof(float) * 4;
        var result = GetArrayWithNewStride(bitangents.Length, oldStride, newStride);

        TransformSpan<Vector4Half, Vector4>(bitangents, result, (bitang) => (Vector4)bitang);

        return result;
    }

    private static byte[] GetArrayWithNewStride(int oldBytesCount, int oldStride, int newStride)
    {
        Debug.Assert(oldBytesCount % oldStride == 0);
        int oldElementsCount = oldBytesCount / oldStride;
        return new byte[oldElementsCount * newStride];
    }

    private static void CalculateTangentsW(Span<Vector4> tangents, Span<Vector4> bitangents, Span<Vector3> normals)
    {
        Debug.Assert(tangents.Length == bitangents.Length && bitangents.Length == normals.Length);

        for (int i = 0; i < tangents.Length; i++)
        {
            ref Vector4 tangent = ref tangents[i];
            Vector4 bitangent = bitangents[i];
            Vector3 normal = normals[i];

            Vector3 vec3Tangent = Unsafe.As<Vector4, Vector3>(ref tangent);
            Vector3 vec3Binorm = Unsafe.As<Vector4, Vector3>(ref bitangent);

            Vector3 tbCross = Vector3.Cross(vec3Tangent, vec3Binorm);
            float handedness = Vector3.Dot(normal, tbCross);

            NormalizeVec4XYZ(ref tangent);
            tangent.W = handedness < 0 ? -1 : 1;
        }
    }

    private static void NormalizeVec4XYZ(ref Vector4 vector)
    {
        Vector3 vec3 = new(vector.X, vector.Y, vector.Z);
        if (vec3.Length() == 0.0f)
            return;

        vec3 = Vector3.Normalize(vec3);
        Debug.Assert(float.IsFinite(vec3.X), "Vec3 not finite: " + vec3);
        Debug.Assert(float.IsFinite(vec3.Y), "Vec3 not finite: " + vec3);
        Debug.Assert(float.IsFinite(vec3.Z), "Vec3 not finite: " + vec3);

        vector = new Vector4(vec3, vector.W);
    }

    private static Vector4 NormalizeSum(Vector4 vector)
    {
        return vector / (vector.X + vector.Y + vector.Z + vector.W);
    }

    private static void TransformSpan<TIn, TOut>(ReadOnlySpan<byte> source, Span<byte> destination, Func<TIn, TOut> transformer)
        where TIn : unmanaged
        where TOut : unmanaged
    {
        Debug.Assert(source.Length % Unsafe.SizeOf<TIn>() == 0);
        Debug.Assert(destination.Length % Unsafe.SizeOf<TOut>() == 0);

        var castSource = MemoryMarshal.Cast<byte, TIn>(source);
        var castDest = MemoryMarshal.Cast<byte, TOut>(destination);
        for (int i = 0; i < castSource.Length; i++)
            castDest[i] = transformer(castSource[i]);
    }
}