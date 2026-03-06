namespace AutoWeeklyCap.UI.Helpers;

public static class InformationTooltip
{
    private static readonly uint QuestionColor = ColorUtils.HexToUInt(0xFF, 0xFF, 0xFF, 0.45f);

    public static void Draw(string tooltip)
    {
        Disabled.Exempt(() =>
        {
            ImGui.SameLine();
            ImGui.TextColored(QuestionColor, "(?)");
            ImGuiEx.Tooltip(tooltip);
        });
    }

    public static void Draw(Action action)
    {
        ImGui.SameLine();
        ImGui.TextColored(QuestionColor, "(?)");

        if (!ImGui.IsItemHovered()) {
            return;
        }

        Disabled.Exempt(() =>
        {
            ImGui.BeginTooltip();
            action();
            ImGui.EndTooltip();
        });
    }
}
