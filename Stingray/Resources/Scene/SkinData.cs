using System.Numerics;

namespace VT2AssetLib.Stingray.Resources.Scene;

internal sealed class SkinData
{
    public Matrix4x4[] InvBindMatrices { get; set; } = null!;

    public uint[] NodeIndices { get; set; } = null!;

    public uint[][] MatrixIndexSets { get; set; } = null!;

    public IEnumerable<(uint NodeIndex, Matrix4x4 InvBindMatrix)> GetJointsBelongingToSet(uint indexSet)
    {
        if (indexSet >= MatrixIndexSets.Length)
            throw new ArgumentOutOfRangeException(nameof(indexSet));

        var setIndices = MatrixIndexSets[indexSet];
        var nodeIndices = setIndices.Select(idx => NodeIndices[idx]);
        var nodeIBMs = setIndices.Select(idx => InvBindMatrices[idx]);

        return nodeIndices.Zip(nodeIBMs);
    }
}