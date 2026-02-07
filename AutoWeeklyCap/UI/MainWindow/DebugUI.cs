using AutoWeeklyCap.Listeners;
using AutoWeeklyCap.Runner;
using AutoWeeklyCap.UI.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.UI.MainWindow;

internal static class DebugUI
{
    internal static void Draw()
    {
        Card.DrawSubtle("Plugin Details", DrawPluginDetails, collapsible: false);
        Card.DrawSubtle("Runner Debug Steps", DrawRunnerDebugSteps, collapsible: false);
        Card.DrawSubtle("Runner Debug Actions", DrawRunnerDebugActions, collapsible: false);
        Card.DrawSubtle("Notification Debug Actions", DrawNotificationDebugActions, collapsible: false);
        Card.DrawSubtle("Game Data State", DrawGameDataState, collapsible: false);
    }

    private static void DrawPluginDetails()
    {
        ImGui.Text($"TaskManager [tasks: {AWC.TaskManager.NumQueuedTasks},current task: {AWC.TaskManager.CurrentTask?.Name ?? "idle"}]");
        ImGui.Text($"Currencies [weekly: {CurrencyHelper.GetWeeklyAcquiredTomestoneCount()}, uncapped: {CurrencyHelper.GetUncappedAcquiredTomestoneCount()}]");
        ImGui.Text($"Restart [recovery: {ClientListener.IsRecoveringFromDisconnect}, restart: {ClientListener.IsRestarting}]");
    }

    private static void DrawRunnerDebugSteps()
    {
        ImGui.Text($"Current stage: {AWC.Runner.GetStatus()}");
        DebugButton("Start", () => AWC.Runner.Start(), false);
        DebugButton("Start on Boot", () => AWC.Runner.AutoStartOnBoot());
        DebugButton("Stop", () => AWC.Runner.Stop());
        DebugButton("Resume", () => AWC.Runner.Resume());
        DebugButton("Abort", () => AWC.Runner.Abort());
    }

    private static void DrawRunnerDebugActions()
    {
        DebugButton("Extract", () => ActionInstance.Extract.Invoke(), false);
        DebugButton("Self Repair", () => ActionInstance.SelfRepair.Invoke());
        DebugButton("NPC Repair", () => ActionInstance.NpcRepair.Invoke());
        DebugButton("Spend Tomestones", () => ActionInstance.SpendTomestone.Invoke());
        DebugButton("Deliveroo", () => ActionInstance.Deliveroo.Invoke());
    }

    private static void DrawNotificationDebugActions()
    {
        DebugButton("Flash Taskbar Icon", () =>
        {
            AWC.TaskManager.EnqueueDelay(1500);
            AWC.TaskManager.Enqueue(NotificationMasterIPC.SendFlashTaskbarIcon);
        }, false);

        DebugButton("Display Toast Notification", () =>
        {
            AWC.TaskManager.EnqueueDelay(1500);
            AWC.TaskManager.Enqueue(() =>
            {
                NotificationMasterIPC.SendDisplayToastNotification(
                    "AWC: Test message title",
                    "AWC: Test message body"
                );
            });
        });

        DebugButton("Play Sound", () =>
        {
            AWC.TaskManager.EnqueueDelay(1500);
            AWC.TaskManager.Enqueue(() =>
            {
                // NotificationMasterIPC.SendPlaySound(
                //     "C:\\Users\\alexis\\Desktop\\Test\\madcow.wav",
                //     .10f,
                //     false,
                //     true
                // );
            });
        });
    }

    private static void DrawGameDataState()
    {
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
