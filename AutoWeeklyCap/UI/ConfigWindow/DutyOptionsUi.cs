using AutoWeeklyCap.Helpers;
using AutoWeeklyCap.Runner;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Bindings.ImGui;

namespace AutoWeeklyCap.UI.ConfigWindow;

public static class DutyOptionsUi
{
    public static void Draw()
    {
        ImGui.TextWrapped("Selected duty");

        if (ImGui.BeginCombo(
                $"###selected-duty",
                TomestoneZone.IsSupportedTomestoneZone(AutoWeeklyCap.Config.ZoneId)
                    ? Utils.GetZoneNameFromId(AutoWeeklyCap.Config.ZoneId)
                    : "Not selected"
            ))
        {
            foreach (var zoneId in TomestoneZone.AvailableTomestoneZones)
            {
                if (ImGui.Selectable(Utils.GetZoneNameFromId(zoneId), AutoWeeklyCap.Config.ZoneId == zoneId))
                {
                    AutoWeeklyCap.Config.ZoneId = zoneId;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.Spacing();
        ImGui.Spacing();

        var stopGracefully = AutoWeeklyCap.Config.StopRunnerGracefully;
        if (ImGui.Checkbox("Stop runs gracefully", ref stopGracefully))
        {
            AutoWeeklyCap.Config.StopRunnerGracefully = stopGracefully;
        }

        InformationTooltip.Draw(
            "When stopping the runner mid duty, graceful stopping will finish the run before stopping completely");

        var useBossModRebornAi = AutoWeeklyCap.Config.UseBossModRebornAI;
        if (ImGui.Checkbox("Use BossMod Reborn AI", ref useBossModRebornAi))
        {
            AutoWeeklyCap.Config.UseBossModRebornAI = useBossModRebornAi;
        }

        InformationTooltip.Draw("When enabled, the BossMod Reborn AI will be used for AutoDuty over the default AI");
    }
}
