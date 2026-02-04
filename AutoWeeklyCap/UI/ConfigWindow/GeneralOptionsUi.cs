using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ConfigWindow;

public static class GeneralOptionsUi
{
    public static void Draw()
    {
        Card.DrawSubtle("General Options", GeneralOptions, collapsible: false);
        Card.DrawSubtle("UI Elements & Windows", UiElementsAndWindows, collapsible: false);
        Card.DrawSubtle("Network Options", NetworkOptions, collapsible: false);
    }

    private static void GeneralOptions()
    {
        var startOnBoot = AWC.Config.StartRunnerOnBoot;
        if (ImGui.Checkbox("Start runner automatically on startup", ref startOnBoot))
            AWC.Config.StartRunnerOnBoot = startOnBoot;

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When the option is enabled and at least one enabled character is still not");
            ImGui.Text("tome capped, AWC will automatically start the runner during game boot.");
        });

        var trackDisabled = AWC.Config.TrackDisabledCharacters;
        if (ImGui.Checkbox("Track tomestones for disabled characters", ref trackDisabled))
            AWC.Config.TrackDisabledCharacters = trackDisabled;
    }

    private static void UiElementsAndWindows()
    {
        var openWindow = AWC.Config.OpenWindowOnStartup;
        if (ImGui.Checkbox("Open Character UI window on startup", ref openWindow))
            AWC.Config.OpenWindowOnStartup = openWindow;

        var useSliders = AWC.Config.UseSliders;
        if (ImGui.Checkbox("Slider inputs", ref useSliders))
            AWC.Config.UseSliders = useSliders;

        InformationTooltip.Draw(
            "When enabled, ranged inputs will be shown as sliders\n" +
            "When disabled, ranged inputs will be shown as text inputs with increment and decrement step buttons"
        );

        var dtrBar = AWC.Config.ShowStatusInStatusBar;
        if (ImGui.Checkbox("Show status in DTR bar", ref dtrBar))
            AWC.Config.ShowStatusInStatusBar = dtrBar;

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Adds a status indicator to the DTR bar, allowing for quickly seeing");
            ImGui.Text("the runner status, and toggling the windows and runner statues");
        });

        Disabled.Draw(!dtrBar, () =>
        {
            ImGui.SameLine(0f, 20f);
            var iconsDtr = AWC.Config.ShowStatusAsIcons;
            if (ImGui.Checkbox("Show status as icons instead of text", ref iconsDtr))
                AWC.Config.ShowStatusAsIcons = iconsDtr;
        });
    }

    private static void NetworkOptions()
    {
        var recovery = AWC.Config.AttemptRecoveryFromDisconnects;
        if (ImGui.Checkbox("Recovery from disconnects", ref recovery))
            AWC.Config.AttemptRecoveryFromDisconnects = recovery;

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

        var titleMovie = AWC.Config.DisableTitleScreenMovie;
        if (ImGui.Checkbox("Disable title screen movie", ref titleMovie))
            AWC.Config.DisableTitleScreenMovie = titleMovie;

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled the title screen movie will disabled, regardless");
            ImGui.Text("of if the runner is actually running or not.");
        });
    }
}
