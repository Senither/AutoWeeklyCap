using AutoWeeklyCap.UI.Helpers;
using Dalamud.Bindings.ImGui;

namespace AutoWeeklyCap.UI.ConfigWindow;

public static class GeneralOptionsUi
{
    public static void Draw()
    {
        var openWindow = AutoWeeklyCap.Config.OpenWindowOnStartup;
        if (ImGui.Checkbox("Open Character UI window on startup", ref openWindow))
            AutoWeeklyCap.Config.OpenWindowOnStartup = openWindow;

        var useSliders = AutoWeeklyCap.Config.UseSliders;
        if (ImGui.Checkbox("Slider inputs", ref useSliders))
            AutoWeeklyCap.Config.UseSliders = useSliders;

        InformationTooltip.Draw(
            "When enabled, ranged inputs will be shown as sliders\n" +
            "When disabled, ranged inputs will be shown as text inputs with increment and decrement step buttons"
        );
    }
}
