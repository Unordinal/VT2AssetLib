namespace VT2AssetLib.Numerics;

// Unused in Vermintide, not sure why I have this in here.
internal static class NormConversions
{
    public static float UnormToFloat(byte value)
    {
        return (float)value / byte.MaxValue;
    }

    public static float UnormToFloat(ushort value)
    {
        return (float)value / ushort.MaxValue; // value / (2^16 - 1)
    }

    public static float UnormToFloat(uint value)
    {
        return (float)value / uint.MaxValue; // value / (2^32 - 1)
    }

    public static float SnormToFloat(sbyte value)
    {
        return MathF.Max((float)value / sbyte.MaxValue, -1.0f);
    }

    public static float SnormToFloat(short value)
    {
        return MathF.Max((float)value / short.MaxValue, -1.0f);
    }

    public static float SnormToFloat(int value)
    {
        return MathF.Max((float)value / int.MaxValue, -1.0f);
    }

    public static byte FloatToUnorm8(float value)
    {
        if (float.IsNaN(value))
            return 0;

        value = Math.Clamp(value, 0.0f, 1.0f);
        value *= byte.MaxValue;
        return (byte)(value + 0.5f);
    }

    public static ushort FloatToUnorm16(float value)
    {
        if (float.IsNaN(value))
            return 0;

        value = Math.Clamp(value, 0.0f, 1.0f);
        value *= ushort.MaxValue;
        return (ushort)(value + 0.5f);
    }

    public static uint FloatToUnorm32(float value)
    {
        if (float.IsNaN(value))
            return 0;

        value = Math.Clamp(value, 0.0f, 1.0f);
        value *= uint.MaxValue;
        return (uint)(value + 0.5f);
    }

    public static sbyte FloatToSnorm8(float value)
    {
        if (float.IsNaN(value))
            return 0;

        value = Math.Clamp(value, 0.0f, 1.0f);
        value *= byte.MaxValue;
        return (sbyte)(value + 0.5f);
    }

    public static short FloatToSnorm16(float value)
    {
        if (float.IsNaN(value))
            return 0;

        value = Math.Clamp(value, -1.0f, 1.0f);
        value *= short.MaxValue;
        return (short)(value + 0.5f);
    }

    public static int FloatToSnorm32(float value)
    {
        if (float.IsNaN(value))
            return 0;

        value = Math.Clamp(value, -1.0f, 1.0f);
        value *= int.MaxValue;
        return (int)(value + 0.5f);
    }
}