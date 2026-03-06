using System.Globalization;

using AutoWeeklyCap.Config;
using AutoWeeklyCap.Listeners;
using AutoWeeklyCap.Runner.Actions;
using AutoWeeklyCap.UI.Helpers;

using ECommons.Logging;

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
        Card.DrawSubtle("Plugin Logs", DrawPluginLogs);
        Card.DrawDanger("Danger Zone", DrawDangerZone);
    }

    private static void DrawPluginDetails()
    {
        ImGui.Text($"TaskManager [tasks: {AWC.TaskManager.NumQueuedTasks},current task: {AWC.TaskManager.CurrentTask?.Name ?? "idle"}]");
        ImGui.Text($"Currencies [weekly: {CurrencyHelper.GetWeeklyAcquiredTomestoneCount()}, uncapped: {CurrencyHelper.GetUncappedAcquiredTomestoneCount()}]");
        ImGui.Text($"Restart [recovery: {ClientListener.IsRecoveringFromDisconnect}, restart: {ClientListener.IsRestarting}]");
        ImGui.Text($"Runner counter [runs: {AWC.Runner.GetRunsCounter()}, character: {AWC.Runner.GetRunsCharacter() ?? "<not set>"}]");
    }

    private static void DrawRunnerDebugSteps()
    {
        ImGui.Text($"Current stage: {TitleManager.GetStatus() ?? "idle"}");
        DebugButton("Start", () => AWC.Runner.Start(), false);
        DebugButton("Start on Boot", () => AWC.Runner.AutoStartOnBoot());
        DebugButton("Stop", () => AWC.Runner.Stop());
        DebugButton("Resume", () => AWC.Runner.Resume());
        DebugButton("Abort", () => AWC.Runner.Abort());
    }

    private static void DrawRunnerDebugActions()
    {
        DebugActionButton("Extract", ActionInstance.Extract, false);
        DebugActionButton("Self Repair", ActionInstance.SelfRepair);
        DebugActionButton("NPC Repair", ActionInstance.NpcRepair);
        DebugActionButton("Spend Tomestones", ActionInstance.SpendTomestone);

        DebugActionButton("Return to Homeworld", ActionInstance.Homeworld, false);
        DebugActionButton("Deliveroo", ActionInstance.Deliveroo);
        DebugActionButton("Notification", ActionInstance.Notification);

        Card.Separator();

        DebugActionButton("Enter GC Inn", ActionInstance.EnterGrandCompanyInn, false);
        DebugActionButton("Leave GC Inn", ActionInstance.LeaveGrandCompanyInn);
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

        DebugButton("Notify: RunnerStopped", () => ActionInstance.Notification.Invoke(StopNotificationType.RunnerStopped), false);
        DebugButton("Notify: CharacterCapped", () => ActionInstance.Notification.Invoke(StopNotificationType.CharacterCapped));

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
        CopyableText(
            $"Position:  T:{Player.Territory.RowId} P:{Player.Position}",
            "position",
            () => $"{Player.Position.X.ToString(CultureInfo.InvariantCulture)}f, {Player.Position.Y.ToString(CultureInfo.InvariantCulture)}f, {Player.Position.Z.ToString(CultureInfo.InvariantCulture)}f"
        );

        var taget = "<no target>";
        uint targetId = 0;
        try {
            unsafe {
                var t = TargetSystem.Instance()->Target;
                var distance = Vector3.Distance(Player.Position, t->Position);

                targetId = t->BaseId;
                taget = $"{t->GetName()} [id: {t->BaseId}, disc: {distance}]";
            }
        } catch (Exception) {
            // ignored
        }

        CopyableText($"Target:     {taget}", "target ID", () => $"{targetId}u");

        ImGui.Text("State:       ");
        StateText(() => PlayerHelper.IsReady, "Ready");
        StateText(() => PlayerHelper.IsValid, "Valid");
        StateText(() => PlayerHelper.IsOccupied, "Occupied");
        StateText(() => PlayerHelper.IsJumping, "Jumping");
        StateText(() => PlayerHelper.IsMoving, "Moving");
        StateText(() => PlayerHelper.IsCasting, "Casting", seperator: false);
    }

    private static void CopyableText(string text, string propertyName, Func<string> copy)
    {
        ImGui.Text(text);

        if (ImGui.IsItemClicked())
            ImGui.SetClipboardText(copy());
        if (ImGui.IsItemHovered())
            ImGuiEx.Tooltip($"Click to copy {propertyName} to clipboard");
    }

    private static void DebugButton(string text, Action action, bool sameLine = true)
    {
        if (sameLine)
            ImGui.SameLine();

        if (ImGui.Button(text))
            action();
    }

    private static void DebugActionButton(string text, BaseAction action, bool sameLine = true)
    {
        if (sameLine)
            ImGui.SameLine();

        if (ImGui.Button(text))
            DuoLog.Warning($"{action.GetName()}: {action.Invoke()}");
    }

    private static void StateText(Func<bool> value, string text, bool seperator = true)
    {
        ImGui.SameLine(0, 0);

        try {
            ImGui.TextColored(value() ? ImGuiColors.HealerGreen : ImGuiColors.DPSRed, text);
        } catch (Exception) {
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);
        }

        if (seperator) {
            ImGui.SameLine(0, 0);
            ImGui.Text(" | ");
        }
    }

    private static void DrawPluginLogs()
    {
        ImGui.BeginChild("DebugPluginLogs", new Vector2(ImGui.GetContentRegionAvail().X - 8, 600), true);
        InternalLog.PrintImgui();
        ImGui.EndChild();
    }

    private static void DrawDangerZone()
    {
        ImGui.TextWrapped("Clicking the button below will reset the main plugin config file to use the default values, it's recommended that you backup the file if you want to save your current settings.");
        ImGui.Spacing();

        if (ImGui.Button("Reset Plugin Config") && ImGuiEx.Ctrl)
            AWC.Instance.Configuration = new Configuration();

        ImGuiEx.Tooltip("Hold down CTRL + Click to reset the plugin configuration to all the default values");
    }
}
