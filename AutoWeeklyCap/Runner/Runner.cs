using AutoWeeklyCap.Runner.Zone;

using Dalamud.Interface.ImGuiNotification;

using ECommons.Automation.NeoTaskManager;
using ECommons.Configuration;

namespace AutoWeeklyCap.Runner;

public class Runner
{
    public RunnerState State { get; } = new();

    public bool Start()
    {
        if (State.CurrentStage != Stage.Waiting || State.StoppingGracefully) {
            return false;
        }

        if (!AWC.Config.IsRequiredSettingsSetup()) {
            return false;
        }

        var character = PlayerHelper.GetFullCharacterName();
        if (character == null || !(AWC.Config.GetOrRegisterCharacterOptions(character)?.IsEnabled() ?? false)) {
            character = null;
        }

        State.SetCurrentCharacter(character);
        State.UpdateTimestamp();
        State.ResetRunsTrackers();

        State.ChangeStageTo(
            character != null && CurrencyHelper.IsPlayerLimitedTomestoneCapped()
                ? Stage.StartingCharacterSwap
                : Stage.PreparingRunner
        );

        AWC.Log.Info("Starting weekly cap runner");

        return true;
    }

    public bool AutoStartOnBoot()
    {
        if (State.CurrentStage != Stage.Waiting || State.StoppingGracefully) {
            return false;
        }

        if (!AWC.Config.IsRequiredSettingsSetup()) {
            return false;
        }

        if (PlayerHelper.IsValid) {
            return false;
        }

        if (AWC.Config.GetFirstUncappedCharacter() == null) {
            return false;
        }

        const int autoStartDelay = 5;
        for (var i = 0; i < autoStartDelay; i++) {
            var seconds = autoStartDelay - i;
            AWC.TaskManager.Enqueue(() => Svc.NotificationManager.AddNotification(new Notification { Content = $"Auto start AWC in {seconds}!", InitialDuration = TimeSpan.FromSeconds(1), HardExpiry = DateTime.Now.AddSeconds(1), Type = NotificationType.Warning }));

            AWC.TaskManager.EnqueueDelay(1000);
        }

        AWC.TaskManager.Enqueue(() =>
        {
            State.ChangeStageTo(Stage.WaitingForAutoRetainer);
            State.UpdateTimestamp();
            State.ResetRunsTrackers();
            State.SetCurrentCharacter(AWC.Config.GetFirstUncappedCharacter());
        });

        return true;
    }

    public void Stop()
    {
        if (PlayerHelper.IsLoggedIn) {
            if (State.CurrentStage is Stage.RunningAutoDuty or Stage.SwitchingCharacter || !AutoDutyIPC.IsStopped()) {
                State.SetStoppingGracefully(true);
                return;
            }
        }

        Abort();
    }

    public void Resume()
    {
        if (!State.StoppingGracefully || AutoDutyIPC.IsStopped()) {
            return;
        }

        if (State.CurrentStage is not (Stage.RunningAutoDuty or Stage.SwitchingCharacter)) {
            return;
        }

        State.SetStoppingGracefully(false);
    }

    public void Abort()
    {
        State.Reset();

        LifestreamIPC.Abort();
        AutoDutyIPC.Stop();
        TitleManager.Reset();
        LocationManager.Reset();
        AWC.TaskManager.Abort();

        AWC.Log.Info("Stopped weekly cap runner");
    }

    public void Tick()
    {
        if (AWC.TaskManager.IsBusy) {
            return;
        }

        switch (State.CurrentStage) {
            case Stage.Waiting:
                break;

            case Stage.PreparingRunner:
                CheckPrerequisitesForRunnerPreparations();
                break;

            case Stage.WaitingForAutoRetainer:
                WaitForAutoRetainer();
                break;

            case Stage.CheckingTomestone:
                CheckTomestoneStage();
                break;

            case Stage.StartingAutoDuty:
                StartAutoDuty();
                break;

            case Stage.RunningAutoDuty:
                RunAutoDuty();
                break;

            case Stage.StartingCharacterSwap:
                StartCharacterSwap();
                break;

            case Stage.SwitchingCharacter:
                SwitchCharacter();
                break;

            case Stage.StoppingRunner:
                StopRunner();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(State.CurrentStage), State.CurrentStage, null);
        }
    }

    private void CheckPrerequisitesForRunnerPreparations()
    {
        if (State.StoppingGracefully) {
            Abort();
            return;
        }

        if (State.CurrentCharacter == null) {
            AWC.Log.Debug($"Runner: Found no character set for, switching stage");
            State.ChangeStageTo(Stage.StartingCharacterSwap);
            return;
        }

        if (AWC.Config.AlwaysStartOnHomeWorld && ActionInstance.Homeworld.Invoke()) {
            return;
        }

        var playerJob = State.LevelingMode
            ? LevelingHelper.GetJobToLevel(State.CurrentCharacter)
            : AWC.Config.GetOrRegisterCharacterOptions(State.CurrentCharacter)?.PreferredJob;

        using (TitleManager.RegisterTitle(playerJob?.GetIcon() ?? BitmapFontIcon.AnyClass, "Switching Job")) {
            if (!playerJob?.IsAlreadyOnJob() ?? false) {
                AWC.TaskManager.Enqueue(() => playerJob?.SwitchToJob() ?? true, "switching job");
                return;
            }
        }

        if (State.LevelingMode && AWC.Config.LevelJobs.BuyExpansionGearUpgrades && ActionInstance.BuyLevelingUpgrade.Invoke()) {
            return;
        }

        if (AWC.Config.AutoRetainerEnabled && AWC.Config.AutoRetainerTrigger.IsWithinThreshold()) {
            State.UpdateTimestamp();
            State.ChangeStageTo(Stage.WaitingForAutoRetainer);
            return;
        }

        AWC.TaskManager.Enqueue(() =>
        {
            if (AutoRetainerIPC.IsEnabled && AutoRetainerIPC.GetMultiModeStatus()) {
                if (!AutoRetainerIPC.IsBusy()) {
                    AutoRetainerIPC.DisableMultiMode();
                }

                return false;
            }

            return true;
        }, "disable AutoRetainer multi mode when it's not busy");

        if (AWC.Config.Extract) {
            ActionInstance.EnqueueAction(ActionInstance.Extract);
        }

        if (AWC.Config.Repair && InventoryHelper.CanRepair(AWC.Config.RepairPercentage)) {
            if (AWC.Config.RepairSelf) {
                ActionInstance.EnqueueAction(ActionInstance.SelfRepair);
            } else {
                ActionInstance.EnqueueAction(ActionInstance.NpcRepair);
            }
        }

        if (AWC.Config.DeliverooEnabled) {
            var shouldRunFirst = AWC.Config.DeliverooRunOnFirstLoop
                                 && State.RunsCounter == 0;

            var shouldRunForCounter = AWC.Config.DeliverooOnInterval
                                      && State.RunsCounter % AWC.Config.DeliverooRunInterval == 0
                                      && State.RunsCounter > 0;

            AWC.Log.Debug($"Runner: Deliveroo check [first: {shouldRunFirst}, forCounter: {shouldRunForCounter}]");
            if (shouldRunFirst || shouldRunForCounter) {
                ActionInstance.EnqueueAction(ActionInstance.Deliveroo);
            }
        }

        if (AWC.Config.SpendUncappedTomestones) {
            if (CurrencyHelper.GetUncappedAcquiredTomestoneCount() >= AWC.Config.SpendUncappedTomestoneThreshold) {
                ActionInstance.EnqueueAction(ActionInstance.SpendTomestone);
            }
        }

        State.ChangeStageTo(Stage.CheckingTomestone);
    }

    private void WaitForAutoRetainer()
    {
        if (!AutoRetainerIPC.GetMultiModeStatus()) {
            AutoRetainerIPC.EnableMultiMode();
        }

        if (AutoRetainerIPC.IsBusy() || LifestreamIPC.IsBusy() || (!PlayerHelper.IsValid && !AddonHelper.IsTitleScreenReady())) {
            State.UpdateTimestamp();
            return;
        }

        var elapsed = (DateTime.UtcNow - State.Timestamp).Seconds;

        switch (PlayerHelper.IsValid) {
            case true when elapsed < 15:
            case false when elapsed < 5:
                return;
        }

        // From this point onwards we're assuming that AutoRetainer has completed its run, next we'll return the original player

        if (State.CurrentCharacter == null) {
            AutoRetainerIPC.DisableMultiMode();
            State.ChangeStageTo(Stage.StartingCharacterSwap);
            return;
        }

        if (PlayerHelper.GetFullCharacterName() == State.CurrentCharacter) {
            State.ChangeStageTo(Stage.PreparingRunner);
            return;
        }

        var limit = CurrencyHelper.GetLimitedTomestoneWeeklyLimit();
        var tomes = AWC.Config.CollectedTomes.GetValueOrDefault(State.CurrentCharacter, 0);
        if (tomes == limit) {
            State.ChangeStageTo(Stage.StartingCharacterSwap);
            return;
        }

        var parts = State.CurrentCharacter.Split("@");

        AWC.Log.Info($"Switching character to {parts[0]} on {parts[1]}");
        State.ChangeStageTo(Stage.SwitchingCharacter);
        State.UpdateTimestamp();
        LifestreamIPC.ChangeCharacter(parts[0], parts[1]);
    }

    private void CheckTomestoneStage()
    {
        var character = PlayerHelper.GetFullCharacterName();
        if (character == null) {
            return;
        }

        var isCapped = CurrencyHelper.IsPlayerLimitedTomestoneCapped();
        if (isCapped && AWC.Config.DeliverooEnabled && !AWC.Config.DeliverooOnInterval) {
            ActionInstance.Deliveroo.Invoke();
        }

        State.UpdateTimestamp();

        if (State.UnlimitedMode) {
            State.ChangeStageTo(Stage.StartingAutoDuty);
            return;
        }

        if (State.LevelingMode) {
            var levelableCharacter = LevelingHelper.GetCharacterToLevel();
            if (levelableCharacter == null) {
                Stop();
            } else if (levelableCharacter == State.CurrentCharacter) {
                State.ChangeStageTo(Stage.StartingAutoDuty);
            } else {
                State.ChangeStageTo(Stage.SwitchingCharacter);
            }

            return;
        }

        State.ChangeStageTo(
            isCapped ? Stage.StartingCharacterSwap : Stage.StartingAutoDuty
        );
    }

    private void StartAutoDuty()
    {
        if (State.CurrentCharacter == null) {
            AWC.Log.Debug("Runner: Stopping runner due to character being NULL");
            Stop();
            return;
        }

        if (AWC.Config.OnlyStartAutoDutyFromSafezone) {
            ActionInstance.Safezone.Invoke(State.CurrentCharacter);
        }

        if (State.LevelingMode && AWC.Config.LevelJobs.UseStylistForGearUpgrades) {
            ActionInstance.EquipGearUpgrade.Invoke();
        }

        AWC.TaskManager.Enqueue(() =>
        {
            if (AWC.Config.UseBossModRebornAI && BossModRebornIPC.IsEnabled) {
                AWC.Log.Debug("Runner: enabling BossMod Reborn AI");
                ChatHelper.RunCommand("bmrai on");
            }
        }, "enable BossMod Reborn AI if option is enabled");

        AWC.TaskManager.Enqueue(LocationManager.RegisterLocation, "register last known location");
        AWC.TaskManager.Enqueue(() => State.UpdateTimestamp(), "set timestamp to track timeouts");

        AWC.TaskManager.Enqueue(() =>
            {
                if (!EzThrottler.Throttle("RunnerStartingDuty", 1000)) {
                    return false;
                }

                TitleManager.Reset();

                if (AWC.ClientState.TerritoryType == DutyZone.GetZoneId(State.LevelingMode)) {
                    AWC.Log.Debug("Runner: Player detected in the duty zone, switching to RunningAutoDuty stage");
                    State.ChangeStageTo(Stage.RunningAutoDuty);

                    State.UpsertCurrentDutyStartUtc(DateTime.UtcNow);

                    if (AutoDutyIPC.IsStopped()) {
                        AutoDutyIPC.Run(DutyZone.GetZoneId(State.LevelingMode), 1, false);
                    }

                    return true;
                }

                if (!PlayerHelper.IsReady || VNavMeshIPC.IsRunning()) {
                    if (EzThrottler.Throttle("RunnerStartingDutyBusyLog", 2500)) {
                        AWC.Log.Debug($"Runner: Resetting AutoDuty start timer, reason: player is busy or VNavMesh is running");
                    }

                    State.UpdateTimestamp();
                    return false;
                }

                unsafe {
                    if ((DateTime.UtcNow - State.Timestamp).Seconds > 5 && !AutoDutyIPC.IsStopped() && AddonHelper.TryGetReadyAddon("Repair", out _)) {
                        AWC.Log.Debug("Runner: Detected repairing attempt to have been idle for 5+ seconds while trying to start AutoDuty");
                        AWC.Log.Debug("Runner: Stopping AutoDuty and repairing through AWC instead, and then restarting");

                        AutoDutyIPC.Stop();
                        AWC.TaskManager.Abort();
                        ActionInstance.SelfRepair.Invoke();

                        State.ChangeStageTo(Stage.CheckingTomestone);

                        return true;
                    }
                }

                if ((DateTime.UtcNow - State.Timestamp).Seconds > 30) {
                    AWC.Log.Debug("Runner: Timed out while trying to start AutoDuty");

                    if (State.CurrentCharacter == null) {
                        AWC.Log.Debug("Runner: Stopping runner due to character being NULL");
                        Stop();
                        return true;
                    }

                    AWC.Log.Debug($"Runner: Disabling AWC for {State.CurrentCharacter} and switching character");

                    AWC.Config.Characters[State.CurrentCharacter].Enabled = false;
                    EzConfig.Save();

                    State.ChangeStageTo(Stage.StartingCharacterSwap);

                    return true;
                }

                if (EzThrottler.Throttle("RunnerStartingDutyStartAttempt", 1500)) {
                    var zoneId = DutyZone.GetZoneId(State.LevelingMode);

                    AWC.Log.Debug(
                        "Runner: Attempting to start AutoDuty: {@Stats}",
                        new Dictionary<string, object> { { "Seconds elapsed", (DateTime.UtcNow - State.Timestamp).Seconds }, { "AutoDuty started", !AutoDutyIPC.IsStopped() }, { "Current zone", AWC.ClientState.TerritoryType }, { "Duty zone", zoneId } }
                    );

                    if (zoneId == 0) {
                        AWC.Log.Debug("Runner: Territory Type ID was detected as zero (0), stopping runner");
                        Stop();
                        return true;
                    }

                    AutoDutyIPC.Run(zoneId, 1, false);
                }

                return false;
            },
            "starting AutoDuty",
            new TaskManagerConfiguration(120_000) // 2 minutes
        );
    }

    private void RunAutoDuty()
    {
        if (!AutoDutyIPC.IsStopped()) {
            return;
        }

        if (AWC.ClientState.TerritoryType == DutyZone.GetZoneId(State.LevelingMode)) {
            return;
        }

        AWC.Log.Debug("Runner: AutoDuty has complete a run, switching to preparations stage");

        if (AWC.Config.UseBossModRebornAI && BossModRebornIPC.IsEnabled) {
            AWC.Log.Debug("Runner: disabling BossMod Reborn AI");
            ChatHelper.RunCommand("bmrai off");
        }

        if (State.LevelingMode && AWC.Config.LevelJobs.UseStylistForGearUpgrades) {
            ActionInstance.EquipGearUpgrade.Invoke();
        }

        if (State.CurrentDutyStartUtc.HasValue && State.CurrentCharacter != null) {
            var durationSeconds = (int)(DateTime.UtcNow - State.CurrentDutyStartUtc.Value).TotalSeconds;
            AWC.Log.Debug($"Runner: Finished the run in {durationSeconds} seconds");

            if (State.IsInNormalMode()) {
                AWC.Config.GetOrRegisterCharacterOptions(State.CurrentCharacter)?.AddDutyDurationSeconds(durationSeconds);
                EzConfig.Save();
            }
        }

        if (State.StoppingGracefully && AWC.Config.NotificationMasterEnabled && AWC.Config.NotificationMasterUsingOnRunnerStopped) {
            ActionInstance.Notification.ForceInvoke(
                State.LevelingMode
                    ? StopNotificationType.LevelingRunStopped
                    : StopNotificationType.RunnerStopped
            );
        }

        State.SetCurrentDutyStartUtc(null);

        if (State.RunsCharacter != State.CurrentCharacter) {
            State.SetRunsCharacter(State.CurrentCharacter);
            State.SetRunsCounter(0);
        }

        State.IncrementRunsCounter();
        State.ChangeStageTo(Stage.PreparingRunner);
    }

    private void StartCharacterSwap()
    {
        var character = AWC.Config.GetFirstUncappedCharacter();
        if (State.IsInNormalMode() && character != null) {
            var parts = character.Split("@");
            if (parts.Length != 2) {
                AWC.Log.Error($"Character {character} is not a valid character name, stopping runner");
                Stop();
                return;
            }

            AWC.Log.Info($"Switching character to {parts[0]} on {parts[1]}");
            State.SetCurrentCharacter(character);
            State.ChangeStageTo(Stage.SwitchingCharacter);
            State.UpdateTimestamp();
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        if (State.RunsCounter > 0 && AWC.Config.NotificationMasterEnabled && AWC.Config.NotificationMasterUsingOnFullyCapped) {
            ActionInstance.Notification.ForceInvoke(StopNotificationType.CharacterCapped);
        }

        if (AWC.Config.StopAction == StopAction.StartUnlimitedRuns) {
            State.EnableUnlimitedMode();
            AWC.Log.Info("All characters have been fully capped, starting unlimited runs");

            var preferredCharacter = AWC.Config.CharacterForSwap;
            if (PlayerHelper.GetFullCharacterName() == preferredCharacter) {
                AWC.Log.Debug("Runner: Player is already on preferred character, starting runner");
                State.SetCurrentCharacter(preferredCharacter);
                State.ChangeStageTo(Stage.PreparingRunner);
                return;
            }

            var parts = preferredCharacter.Split("@");
            if (parts.Length != 2) {
                AWC.Log.Error($"Character {preferredCharacter} is not a valid character name, stopping runner");
                Stop();
                return;
            }

            AWC.Log.Info($"Switching character to {parts[0]} on {parts[1]}");
            State.SetCurrentCharacter(preferredCharacter);
            State.ChangeStageTo(Stage.SwitchingCharacter);
            State.UpdateTimestamp();
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        if (AWC.Config.StopAction == StopAction.LevelJobs) {
            State.EnableLevelingMode();

            var levelableCharacter = LevelingHelper.GetCharacterToLevel();
            if (levelableCharacter == null) {
                AWC.Log.Debug($"Runner: Found no characters to level, stopping runner");
                Stop();
                return;
            }

            if (PlayerHelper.GetFullCharacterName() == levelableCharacter) {
                AWC.Log.Debug("Runner: Player is already on character to level, starting runner");
                State.SetCurrentCharacter(levelableCharacter);
                State.ChangeStageTo(Stage.PreparingRunner);
                return;
            }

            var parts = levelableCharacter.Split("@");
            if (parts.Length != 2) {
                AWC.Log.Error($"Character {levelableCharacter} is not a valid character name, stopping runner");
                Stop();
                return;
            }

            AWC.Log.Info($"Switching character to {parts[0]} on {parts[1]}");
            State.SetCurrentCharacter(levelableCharacter);
            State.ChangeStageTo(Stage.SwitchingCharacter);
            State.UpdateTimestamp();
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        AWC.Log.Info("Found no character with missing weekly capped tomestones, stopping runner");
        State.ChangeStageTo(Stage.StoppingRunner);
    }

    private void SwitchCharacter()
    {
        if (LifestreamIPC.IsBusy()) {
            return;
        }

        var character = PlayerHelper.GetFullCharacterName();
        if (character == null || character != State.CurrentCharacter) {
            return;
        }

        if (!PlayerHelper.IsReady) {
            return;
        }

        AWC.Log.Info("Completed character swap, preparing runner");
        State.ChangeStageTo(Stage.PreparingRunner);
    }

    private void StopRunner()
    {
        Abort();

        AWC.TaskManager.EnqueueDelay(500);
        AWC.TaskManager.Enqueue(
            () => AWC.Config.StopAction.Execute(),
            "executing stop action"
        );
    }
}
