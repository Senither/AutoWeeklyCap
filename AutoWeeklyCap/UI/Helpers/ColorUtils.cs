using System.Globalization;

namespace AutoWeeklyCap.UI.Helpers;

public static class ColorUtils
{
    public static uint HexToUInt(uint r, uint g, uint b, float a = 1f)
    {
        return ImGui.ColorConvertFloat4ToU32(new Vector4(r / 255f, g / 255f, b / 255f, a));
    }

    public static Vector4 HexToVector(uint r, uint g, uint b, float a = 1f)
    {
        return new Vector4(r / 255f, g / 255f, b / 255f, a);
    }

    public static Vector4 HexToVector(string hex, float a = 1f)
    {
        hex = hex.TrimStart('#');

        if (hex.Length != 6) {
            throw new ArgumentException("Hex string must be 6 characters long", nameof(hex));
        }

        return HexToVector(
            uint.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
            uint.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
            uint.Parse(hex.Substring(4, 2), NumberStyles.HexNumber),
            a
        );
    }
}
