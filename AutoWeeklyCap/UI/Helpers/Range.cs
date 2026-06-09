namespace AutoWeeklyCap.UI.Helpers;

public static class Range
{
    public static bool Draw(ImU8String label, scoped ref uint v, uint vMin = 0, uint vMax = 0, ImU8String format = default)
    {
        return DrawWithSteps(label, ref v, vMin, vMax, 1, 10, format);
    }

    public static bool DrawWithSteps(
        ImU8String label,
        scoped ref uint v,
        uint vMin = 0,
        uint vMax = 0,
        uint slowSteps = 1,
        uint fastSteps = 10,
        ImU8String format = default
    )
    {
        if (AWC.Config.UseSliders) {
            return ImGui.SliderUInt(label, ref v, vMin, vMax, format);
        }

        if (!ImGui.InputUInt(label, ref v, slowSteps, fastSteps, format)) {
            return false;
        }

        v = Math.Max(Math.Min(v, vMax), vMin);

        return true;
    }
}
