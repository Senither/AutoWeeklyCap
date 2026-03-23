using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace AutoWeeklyCap.UI.Helpers;

public static class MenuButton
{
    private static readonly Vector2 DefaultPadding = new(14f, 10f);
    private const float IconSpacing = 8f;
    private const float ActiveIndicatorWidth = 6f;
    private const float ActiveIndicatorInset = 2f;
    private const float ActiveOutlineThickness = 1.5f;

    public static bool Draw(FontAwesomeIcon icon, string text, bool isActive = false, string? id = null, Vector2? padding = null)
    {
        var contentPadding = padding ?? DefaultPadding;
        var buttonSize = new Vector2(ImGui.GetContentRegionAvail().X, 0f);
        var buttonId = string.IsNullOrWhiteSpace(id) ? text : id;

        using var buttonColor = ImRaii.PushColor(ImGuiCol.Button, Theme.InteractiveDefault);
        using var hoverColor = ImRaii.PushColor(ImGuiCol.ButtonHovered, Theme.InteractiveHovered);
        using var activeColor = ImRaii.PushColor(ImGuiCol.ButtonActive, Theme.InteractiveActive);
        using var framePadding = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, contentPadding);

        var clicked = ImGui.Button($"##big-icon-button-{buttonId}", buttonSize);

        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var drawList = ImGui.GetWindowDrawList();

        if (isActive) {
            var activeTint = Theme.Accent with { W = 0.12f };

            drawList.AddRectFilled(min, max, ImGui.ColorConvertFloat4ToU32(activeTint), 4f, ImDrawFlags.RoundCornersAll);
            drawList.AddRectFilled(
                new Vector2(min.X, min.Y + ActiveIndicatorInset),
                new Vector2(min.X + ActiveIndicatorWidth, max.Y - ActiveIndicatorInset),
                ImGui.ColorConvertFloat4ToU32(Theme.Accent),
                ActiveIndicatorWidth,
                ImDrawFlags.RoundCornersAll
            );

            drawList.AddRect(
                min,
                max,
                ImGui.ColorConvertFloat4ToU32(Theme.Accent),
                4f,
                ImDrawFlags.RoundCornersAll,
                ActiveOutlineThickness
            );
        }

        var iconText = icon.ToIconString();
        var textSize = ImGui.CalcTextSize(text);

        Vector2 iconSize;
        using (ImRaii.PushFont(UiBuilder.IconFont)) {
            iconSize = ImGui.CalcTextSize(iconText);
        }

        var startX = min.X + contentPadding.X;
        var iconY = min.Y + ((max.Y - min.Y - iconSize.Y) / 2f);
        var textY = min.Y + ((max.Y - min.Y - textSize.Y) / 2f);

        using (ImRaii.PushFont(UiBuilder.IconFont)) {
            drawList.AddText(new Vector2(startX, iconY), ImGui.GetColorU32(ImGuiCol.Text), iconText);
        }

        drawList.AddText(new Vector2(startX + iconSize.X + IconSpacing, textY), ImGui.GetColorU32(ImGuiCol.Text), text);

        return clicked;
    }
}
