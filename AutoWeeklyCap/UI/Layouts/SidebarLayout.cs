using Dalamud.Interface.Utility.Raii;

namespace AutoWeeklyCap.UI.Layouts;

public static class SidebarLayout
{
    private const float SidebarMinWidth = 140f;
    private const float SidebarMaxWidth = 260f;
    private const float SidebarWidthRatio = 0.28f;

    internal static void DrawSidebar(Action render)
    {
        var sidebarWidth = GetSidebarWidth();

        using var sidebarBackground = ImRaii.PushColor(ImGuiCol.ChildBg, Theme.BackgroundDefault);

        using (ImRaii.Child("##awc-sidebar", new Vector2(sidebarWidth, 0f), true)) {
            render();
        }
    }

    internal static void DrawContent(Action render)
    {
        ImGui.SameLine();

        using var sidebarBackground = ImRaii.PushColor(ImGuiCol.ChildBg, Theme.BackgroundDefault);

        using (ImRaii.Child("###Content", new Vector2(0, ImGui.GetContentRegionAvail().Y), true)) {
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
