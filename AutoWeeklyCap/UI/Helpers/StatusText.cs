namespace AutoWeeklyCap.UI.Helpers;

public static class StatusText
{
    public static void Draw(bool status, string text)
    {
        ImGui.SameLine(0f, 0f);
        ImGui.TextColored(status ? Theme.TextSuccess : Theme.TextDanger, text);
        ImGui.SameLine(0f, 0f);
    }
}
