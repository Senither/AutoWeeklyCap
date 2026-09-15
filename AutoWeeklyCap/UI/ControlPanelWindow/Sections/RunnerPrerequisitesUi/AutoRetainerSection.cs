using AutoWeeklyCap.UI.Helpers;

using Range = AutoWeeklyCap.UI.Helpers.Range;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.RunnerPrerequisitesUi;

internal static class AutoRetainerSection
{
    internal static void Draw()
    {
        ImGui.Spacing();

        bool useAutoRetainer = AWC.Config.AutoRetainerEnabled;
        if (ImGui.Checkbox("Use Auto Retainer", ref useAutoRetainer)) {
            AWC.Config.AutoRetainerEnabled = useAutoRetainer;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled and at least one retainer are ready within your selected threshold, the runner will ");
            ImGui.Text("enable MultiMode and then do one full cycle on all your characters before doing another run");

            ImGui.Text("Requires ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" and ");
            StatusText.Draw(AutoRetainerIPC.IsEnabled, "AutoRetainer");
        });

        PluginSettingsButton.Draw(AutoRetainerIPC.Context);

        Disabled.Draw(!AWC.Config.AutoRetainerEnabled, () =>
        {
            ImGui.Text("Wait for up to");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(85 * ImGuiHelpers.GlobalScale);

            var autoRetainerRemainingTime = AWC.Config.AutoRetainerThreshold;
            if (Range.Draw("###AutoRetainerTimeWaitingRange", ref autoRetainerRemainingTime, 0, 300)) {
                AWC.Config.AutoRetainerThreshold = autoRetainerRemainingTime;
            }

            ImGui.SameLine();
            ImGui.Text("seconds");

            ImGui.Text($"Which characters should be within {AWC.Config.AutoRetainerThreshold} seconds?");

            int width = (int)Math.Max(150, ImGui.GetContentRegionAvail().X / 2.5);
            ImGui.SetNextItemWidth(width * ImGuiHelpers.GlobalScale);

            RetainerTrigger retainerTrigger = AWC.Config.AutoRetainerTrigger;
            if (ImGui.BeginCombo("##AutoRetainerTrigger", retainerTrigger.GetName())) {
                foreach (var trigger in Enum.GetValues<RetainerTrigger>()) {
                    if (ImGui.Selectable(trigger.GetName())) {
                        AWC.Config.AutoRetainerTrigger = trigger;
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.Text("Tip");
            InformationTooltip.Draw(() =>
            {
                ImGui.Text("If you're using options such as the Auto Spend Tomestone, Grand Company Deliveries, etc, that moves");
                ImGui.Text("your character around your character might not be near a retainer bell, in order for you to continue");
                ImGui.Text("collecting your retainer ventures it's recommended that you enable Teleportation in AutoRetainer.");
                ImGui.Text("You can enable the option in: AutoRetainer Settings -> Multi Mode -> Common Settings -> Teleportation");
            });

            Card.Separator();

            bool enableOnGracefulStops = AWC.Config.AlwaysEnableAutoRetainerOnGracefulStops;
            if (ImGui.Checkbox("Always enable AutoRetainer when runner stops", ref enableOnGracefulStops)) {
                AWC.Config.AlwaysEnableAutoRetainerOnGracefulStops = enableOnGracefulStops;
            }

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("Will always enable ");
                StatusText.Draw(AutoRetainerIPC.IsEnabled, "AutoRetainer");
                ImGui.Text(" multimode when");
                ImGui.Text("the runner is being stopped gracefully.");
            });
        });
    }
}
