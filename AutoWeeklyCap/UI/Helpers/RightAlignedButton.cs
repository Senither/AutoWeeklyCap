namespace AutoWeeklyCap.UI.Helpers;

public static class RightAlignedButton
{
    public static bool Draw(string text, float offsetX = 0f, float offsetY = 0f)
    {
        ImGui.SameLine();

        var style = ImGui.GetStyle();
        var buttonWidth = ImGui.CalcTextSize(GetVisibleText(text)).X + (style.FramePadding.X * 2f);

        var cursor = ImGui.GetCursorScreenPos();
        var rightBoundX = cursor.X + ImGui.GetContentRegionAvail().X;

        if (Card.TryGetContentRightBoundX(out var cardRightBoundX)) {
            rightBoundX = Math.Min(rightBoundX, cardRightBoundX);
        }

        // @formatter:off
        ImGui.SetCursorScreenPos(cursor with
        {
            X = Math.Max(cursor.X + offsetX, rightBoundX - buttonWidth),
            Y = cursor.Y + offsetY
        });
        // @formatter:on

        return ImGui.Button(text);
    }

    private static string GetVisibleText(string text)
    {
        var idx = text.IndexOf("###", StringComparison.Ordinal);
        return idx < 0 ? text : text[..idx];
    }
}
