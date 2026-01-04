using System;
using Dalamud.Bindings.ImGui;

namespace AutoWeeklyCap.UI.Helpers;

public static class Disabled
{
    internal static bool IsDisabled { get; private set; }

    public static void Draw(Action content) => Draw(true, content);

    public static void Draw(bool isDisabled, Action content)
    {
        var previousState = IsDisabled;

        if (isDisabled)
        {
            ImGui.BeginDisabled();
            IsDisabled = true;
        }

        content.Invoke();

        if (isDisabled)
            ImGui.EndDisabled();

        IsDisabled = previousState;
    }

    public static void Exempt(Action content)
    {
        if (IsDisabled)
            ImGui.EndDisabled();

        content.Invoke();

        if (IsDisabled)
            ImGui.BeginDisabled();
    }
}
