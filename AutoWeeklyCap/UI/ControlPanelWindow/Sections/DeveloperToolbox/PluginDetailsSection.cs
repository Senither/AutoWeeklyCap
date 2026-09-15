using AutoWeeklyCap.Listeners;

using Microsoft.VisualBasic;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.DeveloperToolbox;

internal static class PluginDetailsSection
{
    internal static void Draw()
    {
        string? currencies = Strings.Join([
            $"weekly: {CurrencyHelper.GetWeeklyAcquiredLimitedTomestoneCount()}",
            $"total: {CurrencyHelper.GetTotalAcquiredLimitedTomestoneCount()}",
            $"uncapped: {CurrencyHelper.GetUncappedAcquiredTomestoneCount()}]"
        ], ", ");

        ImGui.Text($"TaskManager [tasks: {AWC.TaskManager.NumQueuedTasks}, current task: {AWC.TaskManager.CurrentTask?.Name ?? "idle"}]");
        ImGui.Text($"Currencies [{currencies}]");
        ImGui.Text($"Restart [recovery: {ClientListener.IsRecoveringFromDisconnect}, restart: {ClientListener.IsRestarting}]");
        ImGui.Text($"Runner counter [current: {AWC.Runner.State.RunsCounter}, session: {AWC.Runner.State.SessionRunsCounter}, character: {AWC.Runner.State.RunsCharacter ?? "<not set>"}]");
        ImGui.Text($"Last known location [{LocationManager.GetLastKnownLocation()?.ToString() ?? "<not set>"}]");
    }
}
