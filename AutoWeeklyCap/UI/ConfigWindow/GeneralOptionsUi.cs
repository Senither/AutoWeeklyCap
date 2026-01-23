using AutoWeeklyCap.IPC;
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

        var recovery = AutoWeeklyCap.Config.AttemptRecoveryFromDisconnects;
        if (ImGui.Checkbox("Recovery from disconnects", ref recovery))
            AutoWeeklyCap.Config.AttemptRecoveryFromDisconnects = recovery;

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled and a disconnect is detected while the runner is active AWC will");
            ImGui.Text("attempt to log back into your character and restart the runner.");
            ImGui.Text("");
            ImGui.Text("Note: It's recommended that ");
            StatusText.Draw(NoKillPlugin.IsEnabled, "No Kill Plugin");
            ImGui.Text(" is enabled when using the feature");
            ImGui.Text("to allow recovering for prolonged internet loss without the game closing");
        });

        var dtrBar = AutoWeeklyCap.Config.ShowStatusInStatusBar;
        if (ImGui.Checkbox("Show status in DTR bar", ref dtrBar))
            AutoWeeklyCap.Config.ShowStatusInStatusBar = dtrBar;

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Adds a status indicator to the DTR bar, allowing for quickly seeing");
            ImGui.Text("the runner status, and toggling the windows and runner statues");
        });

        Disabled.Draw(!dtrBar, () =>
        {
            ImGui.SameLine(0f, 20f);
            var iconsDtr = AutoWeeklyCap.Config.ShowStatusAsIcons;
            if (ImGui.Checkbox("Show status as icons instead of text", ref iconsDtr))
                AutoWeeklyCap.Config.ShowStatusAsIcons = iconsDtr;
        });
    }
}
