using System.Diagnostics.CodeAnalysis;

using Dalamud.Interface;
using Dalamud.Utility;

namespace AutoWeeklyCap.UI.Helpers;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class ThemeButton
{
    public static void Draw(string text, string link)
    {
        Draw(text, () => InteractWithLink(link));
    }

    public static void Draw(string text, Action action)
    {
        using (ApplyThemeStyles()) {
            if (ImGuiEx.Button(text)) {
                action();
            }

            if (ImGui.IsItemHovered()) {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
        }
    }

    public static void Draw(FontAwesomeIcon icon, string text, string link)
    {
        Draw(icon, text, () => InteractWithLink(link));
    }

    public static void Draw(FontAwesomeIcon icon, string text, Action action)
    {
        using (ApplyThemeStyles()) {
            if (ImGuiEx.IconButtonWithText(icon, text)) {
                action();
            }
        }
    }

    private static Theme.ThemeScope ApplyThemeStyles()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(10, 6));
        ImGui.PushStyleColor(ImGuiCol.Button, Theme.InteractiveUnfocused);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Theme.InteractiveActive);
        ImGui.PushStyleColor(ImGuiCol.Border, Theme.InteractiveActive);

        return new Theme.ThemeScope(
            colorCount: 3,
            styleCount: 1
        );
    }

    private static void InteractWithLink(string link)
    {
        if (ImGuiEx.Ctrl) {
            Util.OpenLink(link);
            Notify.Success("Link opened in your browser");
        } else {
            ImGui.SetClipboardText(link);
            Notify.Success("Link copied to clipboard");
        }
    }
}
