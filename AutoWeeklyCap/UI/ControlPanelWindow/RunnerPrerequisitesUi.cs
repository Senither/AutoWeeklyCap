using AutoWeeklyCap.Runner.Zone;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface;

using Range = AutoWeeklyCap.UI.Helpers.Range;

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class RunnerPrerequisitesUi
{
    public static void Draw()
    {
        Card.Draw("Duty Options###runner-duty-options", DrawDutyOptions, defaultOpen: true);
        Card.Draw("Before Runs###runner-before-general", DrawBeforeRunOptions, defaultOpen: true);
        Card.Draw("Between Runs###runner-between-general", DrawBetweenRunsOptions, defaultOpen: true);

        ImGui.TextWrapped("Select how third-party plugins should be integrated into the runner.");
        ImGui.Spacing();

        Card.Draw("Auto Retainer###runner-prereq-auto-retainer", DrawAutoRetainer);
        Card.Draw("Deliveroo###runner-prereq-deliveroo", DrawDeliveroo);
        Card.Draw("Notification Master###runner-prereq-notification-master", DrawNotificationMaster);
    }

    private static void DrawDutyOptions()
    {
        ImGui.TextWrapped("Selected duty");

        if (ImGui.BeginCombo(
                $"###selected-duty",
                TomestoneZone.IsSupportedTomestoneZone(AWC.Config.ZoneId)
                    ? MapHelper.GetZoneNameFromId(AWC.Config.ZoneId)
                    : "Not selected"
            )) {
            foreach (var zoneId in TomestoneZone.AvailableTomestoneZones) {
                if (ImGui.Selectable(MapHelper.GetZoneNameFromId(zoneId), AWC.Config.ZoneId == zoneId)) {
                    AWC.Config.ZoneId = zoneId;
                }
            }

            ImGui.EndCombo();
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Your selected duty is what will be started through ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text(", if your selected duty");

            ImGui.Text("is not unlocked on one of your characters, the runner will automatically");
            ImGui.Text("fallback to the latest duty they have unlocked and run that instead.");
        });

        ImGui.Spacing();
        ImGui.Spacing();

        var useBossModRebornAi = AWC.Config.UseBossModRebornAI;
        if (ImGui.Checkbox("Use BossMod Reborn AI", ref useBossModRebornAi)) {
            AWC.Config.UseBossModRebornAI = useBossModRebornAi;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled, the ");
            StatusText.Draw(BossModRebornIPC.IsEnabled, "BossMod Reborn AI");
            ImGui.Text(" will be used for ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text(" over the default AI");
        });
    }

    private static void DrawBeforeRunOptions()
    {
        // Return to home world
        var returnToHomeWorld = AWC.Config.AlwaysStartOnHomeWorld;
        if (ImGui.Checkbox("Always return to home world", ref returnToHomeWorld)) {
            AWC.Config.AlwaysStartOnHomeWorld = returnToHomeWorld;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled and the runner has detected that your character is not on their");
            ImGui.Text("home world, it will use ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" to teleport back to your home world");
            ImGui.Text("before starting ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text(", ");
            StatusText.Draw(AutoRetainerIPC.IsEnabled, "AutoRetainer");
            ImGui.Text(", ");
            StatusText.Draw(DeliverooIPC.IsEnabled, "Deliveroo");
            ImGui.Text(", etc");
        });

        // Only start from safezone
        var onlyStartAutoDutyFromSafezone = AWC.Config.OnlyStartAutoDutyFromSafezone;
        if (ImGui.Checkbox("Only start AutoDuty from safezone", ref onlyStartAutoDutyFromSafezone)) {
            AWC.Config.OnlyStartAutoDutyFromSafezone = onlyStartAutoDutyFromSafezone;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Moves to your preferred safezone before starting ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text("");

            ImGui.Text("Requires ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" and ");
            StatusText.Draw(VNavMeshIPC.IsEnabled, "VNavMesh");
            ImGui.Text(" to be enabled");
        });

        Disabled.Draw(!onlyStartAutoDutyFromSafezone, () =>
        {
            ImGui.Spacing();
            ImGui.Text("Safezone priority");
            InformationTooltip.Draw("The runner will try these safezones from top to bottom until one can be entered");

            var safezones = AWC.Config.GetSortedSafezones();
            for (var index = 0; index < safezones.Count; index++) {
                var safezone = safezones[index];

                ImGui.PushID($"safezone-priority-{safezone}");

                Disabled.Draw(index == 0, () =>
                {
                    if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowUp)) {
                        AWC.Config.MoveSafezone(safezone, -1);
                    }
                });

                ImGui.SameLine(0f, 4f);
                Disabled.Draw(index == safezones.Count - 1, () =>
                {
                    if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowDown)) {
                        AWC.Config.MoveSafezone(safezone, 1);
                    }
                });

                ImGui.SameLine();
                ImGui.Text(safezone.GetName());

                ImGui.PopID();
            }
        });
    }

    private static void DrawBetweenRunsOptions()
    {
        // Repair (Self & NPC)
        var repairStatus = AWC.Config.Repair;
        if (ImGui.Checkbox("Repair Gear", ref repairStatus)) {
            AWC.Config.Repair = repairStatus;
        }

        Disabled.Draw(!AWC.Config.Repair, () =>
        {
            ImGui.SameLine();
            if (ImGui.RadioButton("Self", AWC.Config.RepairSelf)) {
                AWC.Config.RepairSelf = true;
            }

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("Will use Dark Matter to Self Repair (Requires Leveled Crafters!)");
                ImGui.Text("If self repair is not possible NPC repairs will be used instead");
            });

            ImGui.SameLine();
            if (ImGui.RadioButton("City NPC", !AWC.Config.RepairSelf)) {
                AWC.Config.RepairSelf = false;
            }

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("Will teleport to your grand company and use gil to repair your gear");

                ImGui.Text("Requires ");
                StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
                ImGui.Text(" and ");
                StatusText.Draw(VNavMeshIPC.IsEnabled, "VNavMesh");
                ImGui.Text(" to be enabled");
            });

            ImGui.Text("Trigger @");
            ImGui.SameLine();

            var width = (int)Math.Max(150, ImGui.GetContentRegionAvail().X / 1.5);
            ImGui.PushItemWidth(width * ImGuiHelpers.GlobalScale);

            var autoRepairPercentage = AWC.Config.RepairPercentage;
            if (Range.Draw("##Repair@", ref autoRepairPercentage, 1, 99, "%d%%")) {
                AWC.Config.RepairPercentage = Math.Min(100, Math.Max(1, autoRepairPercentage));
            }

            ImGui.PopItemWidth();

            ImGui.Spacing();
            ImGui.Spacing();
        });

        // Auto Extract
        var autoExtract = AWC.Config.Extract;
        if (ImGui.Checkbox("Extract Materia", ref autoExtract)) {
            AWC.Config.Extract = autoExtract;
        }

        Disabled.Draw(!AWC.Config.Extract, () =>
        {
            ImGui.SameLine(0, 10);
            if (ImGui.RadioButton("Equipped", !AWC.Config.ExtractAll)) {
                AWC.Config.ExtractAll = false;
            }

            ImGui.SameLine(0, 5);
            if (ImGui.RadioButton("All", AWC.Config.ExtractAll)) {
                AWC.Config.ExtractAll = true;
            }
        });

        // Move items between inventory and saddlebag
        var moveDuplicateItemsFromInventoryToSaddlebag = AWC.Config.MoveDuplicateItemsFromInventoryToSaddlebag;
        if (ImGui.Checkbox("Move duplicated stackable items to saddlebag", ref moveDuplicateItemsFromInventoryToSaddlebag)) {
            AWC.Config.MoveDuplicateItemsFromInventoryToSaddlebag = moveDuplicateItemsFromInventoryToSaddlebag;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When this option is enabled, the runner will look for items that are both");
            ImGui.Text("in your inventory and your saddlebag, if the items are stackable the");
            ImGui.Text("runner will try to \"clean up\" your inventory by moving the");
            ImGui.Text("items from the inventory to your saddlebag instead.");
            ImGui.Text("");
            ImGui.Text("It will only move duplicated items that are found in your saddlebag.");
        });

        // Auto Spend Tomestones
        var autoSpendUncappedTomestones = AWC.Config.SpendUncappedTomestones;
        if (ImGui.Checkbox("Auto Spend Uncapped Tomestones", ref autoSpendUncappedTomestones)) {
            AWC.Config.SpendUncappedTomestones = autoSpendUncappedTomestones;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Will teleport to Solution Nine or the Phantom Village and buy");
            ImGui.Text("your selected item with your uncapped tomestones");

            ImGui.Text("Requires ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" and ");
            StatusText.Draw(VNavMeshIPC.IsEnabled, "VNavMesh");
            ImGui.Text(" to be enabled");
        });

        Disabled.Draw(!AWC.Config.SpendUncappedTomestones, () =>
        {
            ImGui.Text("Buy @");
            ImGui.SameLine();

            var width = (int)Math.Max(150, ImGui.GetContentRegionAvail().X / 1.5);
            ImGui.PushItemWidth(width * ImGuiHelpers.GlobalScale);

            var autoBuyWithUncappedTomestones = AWC.Config.SpendUncappedTomestoneThreshold;
            if (Range.Draw("##BuyTomestones@", ref autoBuyWithUncappedTomestones, 1, 2000)) {
                AWC.Config.SpendUncappedTomestoneThreshold = autoBuyWithUncappedTomestones;
            }

            ImGui.PopItemWidth();

            ImGui.Text("Item to buy");
            var selectedItem = TomestoneItemHelper.GetTomestoneItemFromName(AWC.Config.SpendUncappedTomestoneItemName);
            if (ImGui.BeginCombo("##PreferredUncappedTomestoneItem", selectedItem != null ? selectedItem.Name : "Not selected")) {
                foreach (var item in TomestoneItemHelper.GetTomestoneItems()) {
                    if (ImGui.Selectable(item.Name)) {
                        AWC.Config.SpendUncappedTomestoneItemName = item.Name;
                    }
                }

                ImGui.EndCombo();
            }
        });
    }

    private static void DrawAutoRetainer()
    {
        ImGui.Spacing();

        var useAutoRetainer = AWC.Config.AutoRetainerEnabled;
        if (ImGui.Checkbox("Use Auto Retainer", ref useAutoRetainer)) {
            AWC.Config.AutoRetainerEnabled = useAutoRetainer;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled and at least one retainer are ready within your selected threshold, the runner will ");
            ImGui.Text("enable MultiMode and then do one full cycle on all your characters before doing another run");

            ImGui.Text("Requires ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" and ");
            StatusText.Draw(AutoRetainerIPC.IsEnabled, "AutoRetainer");
        });

        DrawPluginSettingsButton(AutoRetainerIPC.Context);

        Disabled.Draw(!AWC.Config.AutoRetainerEnabled, () =>
        {
            var width = (int)Math.Max(150, ImGui.GetContentRegionAvail().X / 2.5);
            ImGui.PushItemWidth(width * ImGuiHelpers.GlobalScale);

            ImGui.Text("Wait for up to");
            ImGui.SameLine();

            var autoRetainerRemainingTime = AWC.Config.AutoRetainerThreshold;
            if (Range.Draw("###AutoRetainerTimeWaitingRange", ref autoRetainerRemainingTime, 0, 300)) {
                AWC.Config.AutoRetainerThreshold = autoRetainerRemainingTime;
            }

            ImGui.SameLine();
            ImGui.Text("seconds");

            ImGui.Text($"Which characters should be within {AWC.Config.AutoRetainerThreshold} seconds?");

            var retainerTrigger = AWC.Config.AutoRetainerTrigger;
            if (ImGui.BeginCombo("##AutoRetainerTrigger", retainerTrigger.GetName())) {
                foreach (var trigger in Enum.GetValues<RetainerTrigger>()) {
                    if (ImGui.Selectable(trigger.GetName())) {
                        AWC.Config.AutoRetainerTrigger = trigger;
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.Text("Tip");
            InformationTooltip.Draw(() =>
            {
                ImGui.Text("If you're using options such as the Auto Spend Tomestone, Grand Company Deliveries, etc, that moves");
                ImGui.Text("your character around your character might not be near a retainer bell, in order for you to continue");
                ImGui.Text("collecting your retainer ventures it's recommended that you enable Teleportation in AutoRetainer.");
                ImGui.Text("You can enable the option in: AutoRetainer Settings -> Multi Mode -> Common Settings -> Teleportation");
            });

            ImGui.PopItemWidth();
        });
    }

    private static void DrawDeliveroo()
    {
        ImGui.Spacing();

        var useDeliveroo = AWC.Config.DeliverooEnabled;
        if (ImGui.Checkbox("Use Deliveroo", ref useDeliveroo)) {
            AWC.Config.DeliverooEnabled = useDeliveroo;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled the runner will move to your grand company and trade in all unused and tradable");
            ImGui.Text("items for grand company seals, and then buy your preferred items (setup within Deliveroo)");

            ImGui.Text("Requires ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" and ");
            StatusText.Draw(VNavMeshIPC.IsEnabled, "VNavMesh");
            ImGui.Text(" to be enabled, along with ");
            StatusText.Draw(DeliverooIPC.IsEnabled, "Deliveroo");
        });

        DrawPluginSettingsButton(DeliverooIPC.Context);

        Disabled.Draw(!AWC.Config.DeliverooEnabled, () =>
        {
            ImGui.Spacing();

            ImGui.Text("When should Deliveroo be used?");

            if (ImGui.RadioButton("After", AWC.Config.DeliverooOnInterval)) {
                AWC.Config.DeliverooOnInterval = true;
            }

            ImGui.SameLine();
            ImGui.PushItemWidth(80 * ImGuiHelpers.GlobalScale);

            var configDeliverooRunInterval = AWC.Config.DeliverooRunInterval;
            if (Range.Draw("runs###deliveroo-run-interval", ref configDeliverooRunInterval, 1, 10)) {
                AWC.Config.DeliverooRunInterval = configDeliverooRunInterval;
            }

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("Runs are only counted on a per-character basis, switching");
                ImGui.Text("between characters will reset the runs counter");
            });

            ImGui.PopItemWidth();

            if (ImGui.RadioButton("After character is tomestone capped", !AWC.Config.DeliverooOnInterval)) {
                AWC.Config.DeliverooOnInterval = false;
            }

            ImGui.Spacing();

            ImGui.Text("How should Deliveroo handle items with materia?");

            var selectedAction = AWC.Config.DeliverooStuckAction;
            foreach (var action in Enum.GetValues<DeliverooStuckAction>()) {
                if (ImGui.RadioButton(action.GetName(), selectedAction == action)) {
                    AWC.Config.DeliverooStuckAction = action;
                }

                InformationTooltip.Draw(action.GetTooltip());
            }

            Card.Separator();
            ImGui.Spacing();

            var runOnFirstLoop = AWC.Config.DeliverooRunOnFirstLoop;
            if (ImGui.Checkbox("Always run before the first AutoDuty run", ref runOnFirstLoop)) {
                AWC.Config.DeliverooRunOnFirstLoop = runOnFirstLoop;
            }
        });
    }

    private static void DrawNotificationMaster()
    {
        ImGui.Spacing();

        var useNotificationMaster = AWC.Config.NotificationMasterEnabled;
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

        DrawPluginSettingsButton(NotificationMasterIPC.Context);

        Disabled.Draw(!AWC.Config.NotificationMasterEnabled, () =>
        {
            ImGui.Spacing();

            ImGui.Text("When do you want to be notified?");

            var usingOnRunnerStopped = AWC.Config.NotificationMasterUsingOnRunnerStopped;
            if (ImGui.Checkbox("When the runner is stopped", ref usingOnRunnerStopped)) {
                AWC.Config.NotificationMasterUsingOnRunnerStopped = usingOnRunnerStopped;
            }

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("This will notify you when the runner is stopped after a duty, this");
                ImGui.Text("only works when you're also using graceful stopping of the runner");
            });

            var usingOnFullyCapped = AWC.Config.NotificationMasterUsingOnFullyCapped;
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

            var usingFlashTaskbarIcon = AWC.Config.NotificationMasterUsingFlashTaskbarIcon;
            if (ImGui.Checkbox("Flash the taskbar icon", ref usingFlashTaskbarIcon)) {
                AWC.Config.NotificationMasterUsingFlashTaskbarIcon = usingFlashTaskbarIcon;
            }

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("When enabled and the game is not the main focus, the taskbar icon will");
                ImGui.Text("begin to flash until you manually focus the game window again");
            });

            var usingToastNotification = AWC.Config.NotificationMasterUsingToastNotification;
            if (ImGui.Checkbox("Send a toast notification", ref usingToastNotification)) {
                AWC.Config.NotificationMasterUsingToastNotification = usingToastNotification;
            }

            var usingPlaySound = AWC.Config.NotificationMasterUsingPlaySound;
            if (ImGui.Checkbox("Play audio track", ref usingPlaySound)) {
                AWC.Config.NotificationMasterUsingPlaySound = usingPlaySound;
            }

            Disabled.Draw(!AWC.Config.NotificationMasterUsingPlaySound, () =>
            {
                var usingPlaySoundOptionFilePath = AWC.Config.NotificationMasterUsingPlaySoundOptionFilePath;
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
                            false, false
                        );
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Stop")) {
                        NotificationMasterIPC.SendStopSound();
                    }
                });

                ImGui.Text("Sound volume:");
                var usingPlaySoundOptionVolume = AWC.Config.NotificationMasterUsingPlaySoundOptionVolume;
                if (Range.Draw("###runner-option-audio-volume", ref usingPlaySoundOptionVolume, 1, 100, "%d%%")) {
                    AWC.Config.NotificationMasterUsingPlaySoundOptionVolume = usingPlaySoundOptionVolume;
                }

                var usingPlaySoundOptionRepeat = AWC.Config.NotificationMasterUsingPlaySoundOptionRepeat;
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

                var usingPlaySoundOptionStopOnFocus = AWC.Config.NotificationMasterUsingPlaySoundOptionStopOnFocus;
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

    private static void DrawPluginSettingsButton(PluginInstallerHelper.PluginContext context)
    {
        var plugin = context.GetExposedPlugin();

        if (plugin is { IsLoaded: true }) {
            if (RightAlignedButton.Draw($"Open Settings###ExposedPluginSettings:{context.PluginName}")) {
                plugin.OpenConfigUi();
            }

            return;
        }

        if (RightAlignedButton.Draw($"Install Plugin###ExposedPluginSettings:{context.PluginName}")) {
            context.InstallPlugin();
        }
    }
}
