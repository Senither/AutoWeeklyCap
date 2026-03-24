using Dalamud.Interface.Utility.Raii;

namespace AutoWeeklyCap.UI.Layouts;

public static class SidebarLayout
{
    private const float SidebarMinWidth = 60f;
    private const float SidebarMaxWidth = 200f;
    private const float SidebarWidthRatio = 0.28f;
    private const float SidebarTextBreakpoint = 180f;

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

    internal static float GetSidebarContentTextBreakpoint()
    {
        var style = ImGui.GetStyle();
        return SidebarTextBreakpoint - (style.WindowPadding.X * 2f);
    }

    private static float GetSidebarWidth()
    {
        var availableWidth = ImGui.GetContentRegionAvail().X;
        var proportionalWidth = availableWidth * SidebarWidthRatio;

        if (proportionalWidth < SidebarTextBreakpoint) {
            return SidebarMinWidth;
        }

        return Math.Clamp(proportionalWidth, SidebarMinWidth, SidebarMaxWidth);
    }
}
