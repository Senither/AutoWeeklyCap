using Dalamud.Bindings.ImGui;
using ECommons.ImGuiMethods;

namespace AutoWeeklyCap.UI.Helpers;

public static class InformationTooltip
{
    public static void Draw(string tooltip)
    {
        ImGui.SameLine();
        ImGui.TextColored(ColorUtils.HexToUInt(0xFF, 0xFF, 0xFF, 0.45f), "(?)");
        ImGuiEx.Tooltip(tooltip);
    }
}
