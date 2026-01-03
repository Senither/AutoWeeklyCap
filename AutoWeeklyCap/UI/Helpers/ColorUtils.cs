using System.Numerics;
using Dalamud.Bindings.ImGui;

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
}
