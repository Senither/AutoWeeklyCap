namespace AutoWeeklyCap.UI.Helpers;

public static class Range
{
    public static bool Draw(ImU8String label, scoped ref uint v, uint vMin = 0, uint vMax = 0, ImU8String format = default)
    {
        if (AWC.Config.UseSliders)
            return ImGui.SliderUInt(label, ref v, vMin, vMax, format: format);

        if (!ImGui.InputUInt(label, ref v, step: 1, stepFast: 10, format: format))
            return false;

        v = Math.Max(Math.Min(v, vMax), vMin);

        return true;
    }
}
