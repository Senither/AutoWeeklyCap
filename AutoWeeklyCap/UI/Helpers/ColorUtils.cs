using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace AutoWeeklyCap.UI.Helpers;

public class ColorUtils
{
    public static uint HexToUInt(uint r, uint g, uint b, float a = 1f)
    {
        return ImGui.ColorConvertFloat4ToU32(new Vector4(r / 255f, g / 255f, b / 255f, a));
    }
}
