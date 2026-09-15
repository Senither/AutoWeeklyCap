using AutoWeeklyCap.UI.Helpers;

using Range = AutoWeeklyCap.UI.Helpers.Range;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.RunnerPrerequisitesUi;

internal static class NotificationMasterSection
{
    internal static void Draw()
    {
        ImGui.Spacing();

        bool useNotificationMaster = AWC.Config.NotificationMasterEnabled;
        if (ImGui.Checkbox("Use Notification Master", ref useNotificationMaster)) {
            AWC.Config.NotificationMasterEnabled = useNotificationMaster;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled the runner will send notifications outside the game");
            ImGui.Text("when the runner has finished capping all your characters");

            ImGui.Text("Requires ");
            StatusText.Draw(NotificationMasterIPC.IsEnabled, "Notification Master");
            ImGui.Text(" to be enabled");
        });

        PluginSettingsButton.Draw(NotificationMasterIPC.Context);

        Disabled.Draw(!AWC.Config.NotificationMasterEnabled, () =>
        {
            ImGui.Spacing();

            ImGui.Text("When do you want to be notified?");

            bool usingOnRunnerStopped = AWC.Config.NotificationMasterUsingOnRunnerStopped;
            if (ImGui.Checkbox("When the runner is stopped", ref usingOnRunnerStopped)) {
                AWC.Config.NotificationMasterUsingOnRunnerStopped = usingOnRunnerStopped;
            }

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("This will notify you when the runner is stopped after a duty, this");
                ImGui.Text("only works when you're also using graceful stopping of the runner");
            });

            bool usingOnFullyCapped = AWC.Config.NotificationMasterUsingOnFullyCapped;
            if (ImGui.Checkbox("When all characters are tome capped", ref usingOnFullyCapped)) {
                AWC.Config.NotificationMasterUsingOnFullyCapped = usingOnFullyCapped;
            }

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("This will notify you when all your enabled characters are fully tome");
                ImGui.Text(" capped, if the stop action is set to unlimited runs it will notify");
                ImGui.Text(" you and then start the unlimited runs afterwards");
            });

            ImGui.Text("How do you want to be notified?");

            bool usingFlashTaskbarIcon = AWC.Config.NotificationMasterUsingFlashTaskbarIcon;
            if (ImGui.Checkbox("Flash the taskbar icon", ref usingFlashTaskbarIcon)) {
                AWC.Config.NotificationMasterUsingFlashTaskbarIcon = usingFlashTaskbarIcon;
            }

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("When enabled and the game is not the main focus, the taskbar icon will");
                ImGui.Text("begin to flash until you manually focus the game window again");
            });

            bool usingToastNotification = AWC.Config.NotificationMasterUsingToastNotification;
            if (ImGui.Checkbox("Send a toast notification", ref usingToastNotification)) {
                AWC.Config.NotificationMasterUsingToastNotification = usingToastNotification;
            }

            bool usingPlaySound = AWC.Config.NotificationMasterUsingPlaySound;
            if (ImGui.Checkbox("Play audio track", ref usingPlaySound)) {
                AWC.Config.NotificationMasterUsingPlaySound = usingPlaySound;
            }

            Disabled.Draw(!AWC.Config.NotificationMasterUsingPlaySound, () =>
            {
                string usingPlaySoundOptionFilePath = AWC.Config.NotificationMasterUsingPlaySoundOptionFilePath;
                ImGui.Text("Select the file that should be played:");

                if (ImGui.InputText("###audio-file-path", ref usingPlaySoundOptionFilePath, 1000)) {
                    AWC.Config.NotificationMasterUsingPlaySoundOptionFilePath = usingPlaySoundOptionFilePath;
                }

                ImGui.SameLine();

                if (FileSelector.Draw("runner-audio-file-selector", ref usingPlaySoundOptionFilePath, filter: FileSelector.AudioFilter)) {
                    AWC.Config.NotificationMasterUsingPlaySoundOptionFilePath = usingPlaySoundOptionFilePath;
                }

                Disabled.Draw(AWC.Config.NotificationMasterUsingPlaySoundOptionFilePath.Length == 0, () =>
                {
                    if (ImGui.Button("Test")) {
                        NotificationMasterIPC.SendPlaySound(
                            AWC.Config.NotificationMasterUsingPlaySoundOptionFilePath,
                            AWC.Config.NotificationMasterUsingPlaySoundOptionVolume / 100f,
                            false,
                            false
                        );
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Stop")) {
                        NotificationMasterIPC.SendStopSound();
                    }
                });

                ImGui.Text("Sound volume:");
                uint usingPlaySoundOptionVolume = AWC.Config.NotificationMasterUsingPlaySoundOptionVolume;
                if (Range.Draw("###runner-option-audio-volume", ref usingPlaySoundOptionVolume, 1, 100, "%d%%")) {
                    AWC.Config.NotificationMasterUsingPlaySoundOptionVolume = usingPlaySoundOptionVolume;
                }

                bool usingPlaySoundOptionRepeat = AWC.Config.NotificationMasterUsingPlaySoundOptionRepeat;
                if (ImGui.Checkbox("Repeat audio track", ref usingPlaySoundOptionRepeat)) {
                    AWC.Config.NotificationMasterUsingPlaySoundOptionRepeat = usingPlaySoundOptionRepeat;
                }

                InformationTooltip.Draw(() =>
                {
                    ImGui.Text("When enabled, the audio track will repeat endlessly until an action is made to stop it, such as");
                    ImGui.Text("using the \"Stop on game focus\" option, or clicking on the \"Stop\" button manually.");
                    ImGui.Text("");
                    ImGui.Text("Note: It's recommended to use the \"Stop on game focus\" option along with the repeat,");
                    ImGui.Text("to prevent the audio from looping endlessly without manual intervetion.");
                });

                ImGui.SameLine();

                bool usingPlaySoundOptionStopOnFocus = AWC.Config.NotificationMasterUsingPlaySoundOptionStopOnFocus;
                if (ImGui.Checkbox("Stop on game focus", ref usingPlaySoundOptionStopOnFocus)) {
                    AWC.Config.NotificationMasterUsingPlaySoundOptionStopOnFocus = usingPlaySoundOptionStopOnFocus;
                }

                InformationTooltip.Draw(() =>
                {
                    ImGui.Text("When enabled, the sound will only be played while the game window is not in focus, and will");
                    ImGui.Text("be stopped automatically as soon as the game window becomes the main focus target");
                });
            }, 12);
        });
    }
}
