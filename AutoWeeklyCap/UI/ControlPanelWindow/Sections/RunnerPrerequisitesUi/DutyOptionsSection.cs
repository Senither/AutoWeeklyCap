using AutoWeeklyCap.Runner.Zone;
using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.RunnerPrerequisitesUi;

internal static class DutyOptionsSection
{
    internal static void Draw()
    {
        ImGui.TextWrapped("Selected duty");

        if (ImGui.BeginCombo(
                $"###selected-duty",
                TomestoneZone.IsSupportedTomestoneZone(AWC.Config.ZoneId)
                    ? MapHelper.GetZoneNameFromId(AWC.Config.ZoneId)
                    : "Not selected"
            )) {
            foreach (var zoneId in TomestoneZone.AvailableTomestoneZones) {
                if (ImGui.Selectable(MapHelper.GetZoneNameFromId(zoneId), AWC.Config.ZoneId == zoneId)) {
                    AWC.Config.ZoneId = zoneId;
                }
            }

            ImGui.EndCombo();
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Your selected duty is what will be started through ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text(", if your selected duty");

            ImGui.Text("is not unlocked on one of your characters, the runner will automatically");
            ImGui.Text("fallback to the latest duty they have unlocked and run that instead.");
        });

        ImGui.Spacing();
        ImGui.Spacing();

        bool useBossModRebornAi = AWC.Config.UseBossModRebornAI;
        if (ImGui.Checkbox("Use BossMod Reborn AI", ref useBossModRebornAi)) {
            AWC.Config.UseBossModRebornAI = useBossModRebornAi;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled, the ");
            StatusText.Draw(BossModRebornIPC.IsEnabled, "BossMod Reborn AI");
            ImGui.Text(" will be used for ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text(" over the default AI");
        });

        bool useAutoDutyProfileOverride = AWC.Config.UseAutoDutyProfileOverride;
        if (ImGui.Checkbox("Use automatic AutoDuty profile", ref useAutoDutyProfileOverride)) {
            AWC.Config.UseAutoDutyProfileOverride = useAutoDutyProfileOverride;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled, the runner will configure your ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text(" profile");
            ImGui.Text("to be optimized for usage with AWC and it's current state by");
            ImGui.Text("enabling and disabling options within ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text(" depending on");
            ImGui.Text("the features you have enabled in the runner.");
            ImGui.Text("");
            ImGui.Text("This feature uses temporary config overrides in ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text(", so");
            ImGui.Text("your settings will automatically be restored when stopped.");
        });
    }
}
