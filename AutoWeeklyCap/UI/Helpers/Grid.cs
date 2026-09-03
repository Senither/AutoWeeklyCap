namespace AutoWeeklyCap.UI.Helpers;

public static class Grid
{
    public static void DrawFixedWidth(string id, IReadOnlyList<Action> elements, float itemWidth, float rowHeight)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(itemWidth, 0f);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(rowHeight, 0f);

        var availableWidth = Math.Max(0f, ImGui.GetContentRegionAvail().X);
        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var cellWidth = Math.Min(itemWidth, availableWidth);
        var columnCount = Math.Max(1, (int)((availableWidth + spacing) / (cellWidth + spacing)));

        Draw(id, elements, columnCount, cellWidth, rowHeight);
    }

    public static void DrawColumns(string id, IReadOnlyList<Action> elements, int columnCount, float rowHeight)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columnCount);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(rowHeight, 0f);

        var availableWidth = Math.Max(0f, ImGui.GetContentRegionAvail().X);
        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var itemWidth = Math.Max(0f, (availableWidth - ((columnCount - 1) * spacing)) / columnCount);

        Draw(id, elements, columnCount, itemWidth, rowHeight);
    }

    private static void Draw(string id, IReadOnlyList<Action> elements, int columnCount, float itemWidth, float rowHeight)
    {
        if (elements.Count == 0) {
            return;
        }

        var spacing = ImGui.GetStyle().ItemSpacing.X;

        for (var index = 0; index < elements.Count; index++) {
            var hasNextInRow = index + 1 < elements.Count && (index + 1) % columnCount != 0;

            ImGui.BeginChild($"##{id}-grid-cell-{index}", new Vector2(itemWidth, rowHeight), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground);
            ImGui.PushID(index);

            elements[index].Invoke();

            ImGui.PopID();
            ImGui.EndChild();

            if (hasNextInRow) {
                ImGui.SameLine(0f, spacing);
            }
        }
    }
}
