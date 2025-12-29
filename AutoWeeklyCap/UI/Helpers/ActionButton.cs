using System;
using Dalamud.Bindings.ImGui;
using ECommons.ImGuiMethods;

namespace AutoWeeklyCap.UI.Helpers;

public static class ActionButton
{
    public static void Draw(string text, string? tooltip, Action action)
    {
        if (ImGui.Button(text) && ImGui.GetIO().KeyCtrl)
            action();

        if (tooltip != null)
            ImGuiEx.Tooltip(tooltip);
    }
}
