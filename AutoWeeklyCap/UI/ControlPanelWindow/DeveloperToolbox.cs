using AutoWeeklyCap.UI.ControlPanelWindow.Sections.DeveloperToolbox;
using AutoWeeklyCap.UI.Helpers;

using ECommons.Logging;

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class DeveloperToolbox
{
    internal static void Draw()
    {
        Card.Draw("Plugin Details", PluginDetailsSection.Draw, false);
        Card.Draw("Runner Debug Steps", RunnerDebugStepsSection.Draw, false);
        Card.Draw("Runner Debug Actions", RunnerDebugActionsSection.Draw, false);
        Card.Draw("AutoDuty Config Overrides", AutoDutyConfigOverridesSection.Draw, false);
        Card.Draw("Notification Debug Actions", NotificationDebugActionsSection.Draw, false);
        Card.Draw("Game Data State", GameDataStateSection.Draw, false);
        Card.Draw("Plugin Logs", DrawPluginLogs);
        Card.Draw("Addon Debug Actions", AddonDebugActionsSection.Draw);
        Card.DrawDanger("Plugin Configuration", PluginConfigurationSection.Draw);
    }

    private static void DrawPluginLogs()
    {
        ImGui.BeginChild("DebugPluginLogs", new Vector2(ImGui.GetContentRegionAvail().X - 8, 600), true);
        InternalLog.PrintImgui();
        ImGui.EndChild();
    }
}
