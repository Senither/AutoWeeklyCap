using System.Diagnostics.CodeAnalysis;

using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.Layouts;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
internal static class Theme
{
    internal static Vector4 Primary => AWC.Config.SelectedColorTheme.GetPrimaryColor();

    internal static Vector4 InteractiveDefault = ColorUtils.HexToVector("#444444");
    internal static Vector4 InteractiveHovered => ColorUtils.DarkenVector4(Primary, 0.15f) with { W = 0.85f };
    internal static Vector4 InteractiveActive => ColorUtils.DarkenVector4(Primary, 0.15f);
    internal static Vector4 InteractiveUnfocused => ColorUtils.DarkenVector4(Primary, 0.10f) with { W = 0.7f };
    internal static Vector4 InteractiveLighter => ColorUtils.DarkenVector4(Primary, -0.75f);

    internal static Vector4 ButtonSuccess = ColorUtils.HexToVector("#007009");
    internal static Vector4 ButtonSuccessHovered = ColorUtils.HexToVector("#007009", 0.65f);

    internal static Vector4 BackgroundDefault = ColorUtils.HexToVector("#161616");
    internal static Vector4 BackgroundMedium = ColorUtils.HexToVector("#0C0C0C");
    internal static Vector4 BackgroundDark = ColorUtils.HexToVector("#0A0A0A");
    internal static Vector4 BackgroundWarning = ColorUtils.HexToVector("#C08C1D");
    internal static Vector4 BackgroundDanger = ColorUtils.HexToVector("#FF3C3C");

    internal static Vector4 BorderDefault = ColorUtils.HexToVector("#262626");
    internal static Vector4 BorderDark = ColorUtils.HexToVector("#262626");
    internal static Vector4 BorderWarning = ColorUtils.HexToVector("#AB8E1B", 0.8f);
    internal static Vector4 BorderDanger = ColorUtils.HexToVector("#AB1B1B", 0.8f);

    internal static Vector4 TextDefault = ColorUtils.HexToVector("#FAFAFA");
    internal static Vector4 TextPrimary => ColorUtils.DarkenVector4(Primary, -0.55f) with { W = 0.75f };
    internal static Vector4 TextSuccess = ColorUtils.HexToVector("#00CC22");
    internal static Vector4 TextWarning = ColorUtils.HexToVector("#EBD22A");
    internal static Vector4 TextDanger = ColorUtils.HexToVector("#CC0000");
    internal static Vector4 TextMuted = ColorUtils.HexToVector("#FFFFFF", 0.45f);

    internal static IDisposable Push(bool withBackground = true)
    {
        var colorCount = 0;
        var styleCount = 0;

        foreach (var (color, value) in GetThemeColors()) { PushColor(color, value); }

        foreach (var (styleVar, value) in GetThemeStyles()) { PushVar(styleVar, value); }

        if (!withBackground) {
            return new ThemeScope(colorCount, styleCount);
        }

        var backgrounds = PushBackgroundColors();

        return new ThemeScope(colorCount + backgrounds.ColorCount, styleCount + backgrounds.StyleCount);

        void PushVar(ImGuiStyleVar styleVar, float value)
        {
            ImGui.PushStyleVar(styleVar, value);
            styleCount++;
        }

        void PushColor(ImGuiCol color, Vector4 value)
        {
            ImGui.PushStyleColor(color, value);
            colorCount++;
        }
    }

    private static ThemeScope PushBackgroundColors()
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg, BackgroundDefault);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, BackgroundDefault);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, BackgroundDefault);

        return new ThemeScope(3, 0);
    }

    internal static IDisposable PushSuccessButton()
    {
        ImGui.PushStyleColor(ImGuiCol.Button, ButtonSuccess);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ButtonSuccessHovered);

        return new ThemeScope(2, 0);
    }

    private static (ImGuiCol Color, Vector4 Value)[] GetThemeColors()
    {
        return
        [
            (ImGuiCol.ScrollbarBg, BackgroundDark),

            (ImGuiCol.Border, BorderDark),
            (ImGuiCol.Separator, BorderDark),

            (ImGuiCol.Text, TextDefault),
            (ImGuiCol.TextDisabled, TextMuted),
            (ImGuiCol.Button, InteractiveDefault),

            (ImGuiCol.ButtonHovered, InteractiveHovered),
            (ImGuiCol.ButtonActive, InteractiveActive),
            (ImGuiCol.Tab, InteractiveDefault),
            (ImGuiCol.TabHovered, InteractiveHovered),
            (ImGuiCol.TabActive, InteractiveActive),
            (ImGuiCol.TabUnfocused, InteractiveUnfocused),
            (ImGuiCol.TabUnfocusedActive, InteractiveUnfocused),

            (ImGuiCol.CheckMark, InteractiveLighter),
            (ImGuiCol.SliderGrab, InteractiveLighter),
        ];
    }

    private static (ImGuiStyleVar StyleVar, float Value)[] GetThemeStyles()
    {
        return
        [
            (ImGuiStyleVar.WindowRounding, 8f),
            (ImGuiStyleVar.ChildRounding, 6f),
            (ImGuiStyleVar.FrameRounding, 4f),
            (ImGuiStyleVar.ScrollbarRounding, 4f),
            (ImGuiStyleVar.GrabRounding, 4f),
            (ImGuiStyleVar.TabRounding, 6f),
            (ImGuiStyleVar.FrameBorderSize, 0.2f)
        ];
    }

    public class ThemeScope(int colorCount, int styleCount) : IDisposable
    {
        public readonly int ColorCount = colorCount;
        public readonly int StyleCount = styleCount;

        public void Dispose()
        {
            ImGui.PopStyleColor(ColorCount);
            ImGui.PopStyleVar(StyleCount);
        }
    }
}
