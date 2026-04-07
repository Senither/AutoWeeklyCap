using System.Diagnostics.CodeAnalysis;

using Dalamud.Interface;

namespace AutoWeeklyCap.UI.Helpers;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class ThemeButton
{
    public static void Draw(string text, string linkToCopy)
    {
        Draw(text, () =>
        {
            ImGui.SetClipboardText(linkToCopy);
            Notify.Success("Link copied to clipboard");
        });
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

    public static void Draw(FontAwesomeIcon icon, string text, string linkToCopy)
    {
        Draw(icon, text, () =>
        {
            ImGui.SetClipboardText(linkToCopy);
            Notify.Success("Link copied to clipboard");
        });
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
}
