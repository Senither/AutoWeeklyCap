using Dalamud.Interface.Utility.Raii;

namespace AutoWeeklyCap.UI.Layouts;

public static class SidebarLayout
{
    private const float SidebarMinWidth = 200f;
    private const float SidebarMaxWidth = 220f;
    private const float SidebarWidthRatio = 0.28f;

    internal static void DrawSidebar(Action render)
    {
        var sidebarWidth = GetSidebarWidth();

        using var background = ImRaii.PushColor(ImGuiCol.ChildBg, Theme.BackgroundDefault);

        using (ImRaii.Child("##awc-sidebar", new Vector2(sidebarWidth, 0f), true)) {
            render();
        }
    }

    internal static void DrawContent(Action render)
    {
        ImGui.SameLine();

        using var background = ImRaii.PushColor(ImGuiCol.ChildBg, Theme.BackgroundMedium);

        using (ImRaii.Child("###awc-content", new Vector2(0, ImGui.GetContentRegionAvail().Y), true)) {
            render();
        }
    }

    private static float GetSidebarWidth()
    {
        var availableWidth = ImGui.GetContentRegionAvail().X;
        var proportionalWidth = availableWidth * SidebarWidthRatio;
        return Math.Clamp(proportionalWidth, SidebarMinWidth, SidebarMaxWidth);
    }
}
