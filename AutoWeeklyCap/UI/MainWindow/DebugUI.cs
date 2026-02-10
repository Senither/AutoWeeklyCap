using AutoWeeklyCap.Listeners;
using AutoWeeklyCap.Runner;
using AutoWeeklyCap.Runner.Actions;
using AutoWeeklyCap.UI.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Range = AutoWeeklyCap.UI.Helpers.Range;

// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.UI.MainWindow;

internal static class DebugUI
{
    private static string DebugAudioFilePath = "";
    private static uint DebugAudioVolume = 50;
    private static bool DebugAudioRepeat = false;
    private static bool DebugAudioStopOnFocus = false;

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

        DebugButton("Deliveroo", () => ActionInstance.Deliveroo.Invoke(), false);
        DebugButton("Notification", () => ActionInstance.Notification.Invoke());
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
                NotificationMasterIPC.SendPlaySound(
                    DebugAudioFilePath,
                    DebugAudioVolume / 100f,
                    DebugAudioRepeat,
                    DebugAudioStopOnFocus
                );
            });
        });

        DebugButton("Stop Sound", () => AWC.TaskManager.Enqueue(NotificationMasterIPC.SendStopSound));

        DebugButton("Notify: RunnerStopped", () => ActionInstance.Notification.Invoke(NotificationType.RunnerStopped), false);
        DebugButton("Notify: CharacterCapped", () => ActionInstance.Notification.Invoke(NotificationType.CharacterCapped));

        ImGui.Spacing();
        ImGui.TextWrapped("All actions except for \"Stop Sound\" has a 1500ms delay");

        Card.Separator();

        ImGui.Text("Select the file that should be played:");
        ImGui.InputText("###audio-file-path", ref DebugAudioFilePath, 1000);
        ImGui.SameLine();
        FileSelector.Draw("debug-audio-file-selector", ref DebugAudioFilePath, filter: FileSelector.AudioFilter);
        ImGui.Spacing();

        ImGui.Text("Audio volume:");
        Range.Draw("###audio-volume", ref DebugAudioVolume, 1, 100, "%d%%");
        ImGui.Spacing();

        ImGui.Text("Audio options:");
        ImGui.Checkbox("Should repeat", ref DebugAudioRepeat);
        ImGui.SameLine();
        ImGui.Checkbox("Should stop on focus", ref DebugAudioStopOnFocus);
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
