namespace AutoWeeklyCap.UI.Helpers;

public static class InformationTooltip
{
    public static void Draw(string tooltip)
    {
        Disabled.Exempt(() =>
        {
            ImGui.SameLine();
            ImGui.TextColored(Theme.TextMuted, "(?)");
            ImGuiEx.Tooltip(tooltip);
        });
    }

    public static void Draw(Action action)
    {
        ImGui.SameLine();
        ImGui.TextColored(Theme.TextMuted, "(?)");

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
