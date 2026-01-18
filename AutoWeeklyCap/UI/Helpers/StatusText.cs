using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;

namespace AutoWeeklyCap.UI.Helpers;

public static class StatusText
{
    public static void Draw(bool status, string text)
    {
        ImGui.SameLine(0f, 0f);
        ImGui.TextColored(status ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed, text);
        ImGui.SameLine(0f, 0f);
    }
}
