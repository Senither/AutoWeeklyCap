using AutoWeeklyCap.UI.Helpers;

using Range = AutoWeeklyCap.UI.Helpers.Range;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.DeveloperToolbox;

internal static class NotificationDebugActionsSection
{
    private static string DebugAudioFilePath = "";
    private static uint DebugAudioVolume = 50;
    private static bool DebugAudioRepeat = false;
    private static bool DebugAudioStopOnFocus = false;

    internal static void Draw()
    {
        DebugButton.Draw("Flash Taskbar Icon", () =>
        {
            AWC.TaskManager.EnqueueDelay(1500);
            AWC.TaskManager.Enqueue(NotificationMasterIPC.SendFlashTaskbarIcon);
        }, false);

        DebugButton.Draw("Display Toast Notification", () =>
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

        DebugButton.Draw("Play Sound", () =>
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

        DebugButton.Draw("Stop Sound", () => AWC.TaskManager.Enqueue(NotificationMasterIPC.SendStopSound));

        DebugButton.Draw("Notify: RunnerStopped", () => ActionInstance.Notification.Invoke(StopNotificationType.RunnerStopped), false);
        DebugButton.Draw("Notify: CharacterCapped", () => ActionInstance.Notification.Invoke(StopNotificationType.CharacterCapped));
        DebugButton.Draw("Notify: LevelingStopped", () => ActionInstance.Notification.Invoke(StopNotificationType.LevelingRunStopped));

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
}
