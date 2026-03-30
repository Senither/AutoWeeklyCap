namespace AutoWeeklyCap.Enums;

public enum StatusOverlayPosition
{
    TopLeft = 0,
    TopCenter = 1,
    TopRight = 2,

    MiddleLeft = 10,
    MiddleCenter = 11,
    MiddleRight = 12,

    BottomLeft = 20,
    BottomCenter = 21,
    BottomRight = 22,
}

public static class StatusOverlayPositionExtensions
{
    public static Vector2 GetVector2(this StatusOverlayPosition position)
    {
        const float padding = 20f;

        var viewport = ImGuiHelpers.MainViewport;
        var viewportPos = viewport.Pos;
        var viewportSize = viewport.Size;
        var windowSize = ImGui.GetWindowSize();

        var left = viewportPos.X + padding;
        var centerX = viewportPos.X + (viewportSize.X / 2) - (windowSize.X / 2);
        var right = viewportPos.X + viewportSize.X - windowSize.X - padding;

        var top = viewportPos.Y + padding;
        var centerY = viewportPos.Y + (viewportSize.Y / 2) - (windowSize.Y / 2);
        var bottom = viewportPos.Y + viewportSize.Y - windowSize.Y - padding;

        return position switch
        {
            StatusOverlayPosition.TopLeft => new Vector2(left, top),
            StatusOverlayPosition.TopCenter => new Vector2(centerX, top),
            StatusOverlayPosition.TopRight => new Vector2(right, top),

            StatusOverlayPosition.MiddleLeft => new Vector2(left, centerY),
            StatusOverlayPosition.MiddleCenter => new Vector2(centerX, centerY),
            StatusOverlayPosition.MiddleRight => new Vector2(right, centerY),

            StatusOverlayPosition.BottomLeft => new Vector2(left, bottom),
            StatusOverlayPosition.BottomCenter => new Vector2(centerX, bottom),
            StatusOverlayPosition.BottomRight => new Vector2(right, bottom),

            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };
    }
}
