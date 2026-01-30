using AutoWeeklyCap.Listeners;
using AutoWeeklyCap.Runner;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace AutoWeeklyCap.UI.MainWindow;

internal static class DebugUI
{
    internal static void Draw()
    {
        ImGui.Text($"TaskManager [tasks: {AWC.TaskManager.NumQueuedTasks},current task: {AWC.TaskManager.CurrentTask?.Name ?? "idle"}]");
        ImGui.Text($"Currencies [weekly: {CurrencyHelper.GetWeeklyAcquiredTomestoneCount()}, uncapped: {CurrencyHelper.GetUncappedAcquiredTomestoneCount()}]");
        ImGui.Text($"Restart [recovery: {ClientListener.IsRecoveringFromDisconnect}, restart: {ClientListener.IsRestarting}]");

        ImGui.Separator();

        ImGui.Text($"Runner debug steps [stage: {AWC.Runner.GetStatus()}]");
        DebugButton("Start", () => AWC.Runner.Start(), false);
        DebugButton("Stop", () => AWC.Runner.Stop());
        DebugButton("Resume", () => AWC.Runner.Resume());
        DebugButton("Abort", () => AWC.Runner.Abort());

        ImGui.Separator();

        ImGui.Text($"Runner actions:");
        DebugButton("Extract", () => ActionInstance.Extract.Invoke(), false);
        DebugButton("Self Repair", () => ActionInstance.SelfRepair.Invoke());
        DebugButton("NPC Repair", () => ActionInstance.NpcRepair.Invoke());
        DebugButton("Spend Tomestones", () => ActionInstance.SpendTomestone.Invoke());
        DebugButton("Deliveroo", () => ActionInstance.Deliveroo.Invoke());

        ImGui.Separator();
        ImGui.Text("Game data state:");
        ImGui.Text($"Position:  T:{Player.Territory.RowId} P:{Player.Position}");

        var taget = "<no target>";
        try
        {
            unsafe
            {
                var t = TargetSystem.Instance()->Target;

                taget = $"{t->GetName()} [id: {t->BaseId}]";
            }
        }
        catch (Exception)
        {
            // ignored
        }

        ImGui.Text($"Target:     {taget}");
        ImGui.Text("State:       ");
        StateText(() => PlayerHelper.IsReady, "Ready");
        StateText(() => PlayerHelper.IsValid, "Valid");
        StateText(() => PlayerHelper.IsOccupied, "Occupied");
        StateText(() => PlayerHelper.IsJumping, "Jumping");
        StateText(() => PlayerHelper.IsMoving, "Moving");
        StateText(() => PlayerHelper.IsCasting, "Casting", seperator: false);
    }

    private static void DebugButton(string text, Action action, bool sameLine = true)
    {
        if (sameLine)
            ImGui.SameLine();

        if (ImGui.Button(text))
            action();
    }

    private static void StateText(Func<bool> value, string text, bool seperator = true)
    {
        ImGui.SameLine(0, 0);

        try
        {
            ImGui.TextColored(value() ? ImGuiColors.HealerGreen : ImGuiColors.DPSRed, text);
        }
        catch (Exception)
        {
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);
        }

        if (seperator)
        {
            ImGui.SameLine(0, 0);
            ImGui.Text(" | ");
        }
    }
}
