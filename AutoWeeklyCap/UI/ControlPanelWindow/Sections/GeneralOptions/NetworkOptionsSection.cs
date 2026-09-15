using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.GeneralOptions;

internal static class NetworkOptionsSection
{
    internal static void Draw()
    {
        bool recovery = AWC.Config.AttemptRecoveryFromDisconnects;
        if (ImGui.Checkbox("Recovery from disconnects", ref recovery)) {
            AWC.Config.AttemptRecoveryFromDisconnects = recovery;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled and a disconnect is detected while the runner is active, AWC will");
            ImGui.Text("attempt to log back into your character and restart the runner.");
            ImGui.Text("");
            ImGui.Text("Note: It's recommended that ");
            StatusText.Draw(NoKillPluginIPC.IsEnabled, "No Kill Plugin");
            ImGui.Text(" is enabled when using the feature");
            ImGui.Text("to allow recovering from prolonged internet loss without the game closing");
        });

        bool titleMovie = AWC.Config.DisableTitleScreenMovie;
        if (ImGui.Checkbox("Disable title screen movie", ref titleMovie)) {
            AWC.Config.DisableTitleScreenMovie = titleMovie;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled the title screen movie will be disabled, regardless");
            ImGui.Text("of if the runner is actually running or not.");
        });
    }
}
