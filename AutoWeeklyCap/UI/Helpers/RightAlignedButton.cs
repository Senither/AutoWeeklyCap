using Dalamud.Bindings.ImGui;

namespace AutoWeeklyCap.UI.Helpers;

public static class RightAlignedButton
{
    public static bool Draw(string text)
    {
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X);

        return ImGui.Button(text);
    }
}
