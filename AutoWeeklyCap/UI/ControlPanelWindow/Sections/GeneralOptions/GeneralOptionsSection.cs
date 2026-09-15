using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.GeneralOptions;

internal static class GeneralOptionsSection
{
    internal static void Draw()
    {
        bool startOnBoot = AWC.Config.StartRunnerOnBoot;
        if (ImGui.Checkbox("Start runner automatically on startup", ref startOnBoot)) {
            AWC.Config.StartRunnerOnBoot = startOnBoot;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When the option is enabled and at least one enabled character is still not");
            ImGui.Text("tome capped, AWC will automatically start the runner during game boot.");
            ImGui.Text("");
            ImGui.Text("Note: If all enabled characters are tome caped and unlimited mode is enabled");
            ImGui.Text("the runner will still not be automatically started during game boot.");
        });

        bool trackDisabled = AWC.Config.TrackDisabledCharacters;
        if (ImGui.Checkbox("Track tomestones for disabled characters", ref trackDisabled)) {
            AWC.Config.TrackDisabledCharacters = trackDisabled;
        }
    }
}
