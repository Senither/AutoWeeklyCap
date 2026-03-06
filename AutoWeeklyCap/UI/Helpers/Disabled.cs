namespace AutoWeeklyCap.UI.Helpers;

public static class Disabled
{
    internal static bool IsDisabled { get; private set; }

    public static void Draw(Action content) => Draw(true, content);

    public static void Draw(Action content, int indent) => Draw(true, content, indent);

    public static void Draw(bool isDisabled, Action content)
    {
        Draw(isDisabled, content, 0);
    }

    public static void Draw(bool isDisabled, Action content, int indent)
    {
        var previousState = IsDisabled;
        var shouldIndent = indent > 0;
        var previousCursorPosX = shouldIndent ? ImGui.GetCursorPosX() : 0;

        if (isDisabled) {
            ImGui.BeginDisabled();
            IsDisabled = true;
        }

        if (shouldIndent) {
            ImGui.SetCursorPosX(previousCursorPosX + indent);
            ImGui.BeginGroup();
        }

        content.Invoke();

        if (shouldIndent) {
            ImGui.EndGroup();
            ImGui.SetCursorPosX(previousCursorPosX);
        }

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
