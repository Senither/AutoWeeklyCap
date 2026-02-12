using AutoWeeklyCap.Runner;
using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ConfigWindow;

public static class DutyOptionsUi
{
    public static void Draw()
    {
        ImGui.TextWrapped("Selected duty");

        if (ImGui.BeginCombo(
                $"###selected-duty",
                TomestoneZone.IsSupportedTomestoneZone(AWC.Config.ZoneId)
                    ? MapHelper.GetZoneNameFromId(AWC.Config.ZoneId)
                    : "Not selected"
            ))
        {
            foreach (var zoneId in TomestoneZone.AvailableTomestoneZones)
            {
                if (ImGui.Selectable(MapHelper.GetZoneNameFromId(zoneId), AWC.Config.ZoneId == zoneId))
                    AWC.Config.ZoneId = zoneId;
            }

            ImGui.EndCombo();
        }

        ImGui.Spacing();
        ImGui.Spacing();

        var stopGracefully = AWC.Config.StopRunnerGracefully;
        if (ImGui.Checkbox("Stop runs gracefully", ref stopGracefully))
            AWC.Config.StopRunnerGracefully = stopGracefully;

        InformationTooltip.Draw("When stopping the runner mid duty, graceful stopping will finish the run before stopping completely");

        var useBossModRebornAi = AWC.Config.UseBossModRebornAI;
        if (ImGui.Checkbox("Use BossMod Reborn AI", ref useBossModRebornAi))
            AWC.Config.UseBossModRebornAI = useBossModRebornAi;

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled, the ");
            StatusText.Draw(BossModRebornIPC.IsEnabled, "BossMod Reborn AI");
            ImGui.Text(" will be used for ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text(" over the default AI");
        });

        var returnToHomeWorld = AWC.Config.AlwaysStartOnHomeWorld;
        if (ImGui.Checkbox("Always return to home world", ref returnToHomeWorld))
            AWC.Config.AlwaysStartOnHomeWorld = returnToHomeWorld;

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled and the runner has detected that your character is not on");
            ImGui.Text("their home world, it will use ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" to teleport back to your home");
            ImGui.Text("world before starting ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text(", ");
            StatusText.Draw(AutoRetainerIPC.IsEnabled, "AutoRetainer");
            ImGui.Text(", etc");
        });
    }
}
