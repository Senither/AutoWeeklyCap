using AutoWeeklyCap.Runner.Zone;

using Dalamud.Interface.ImGuiNotification;

using ECommons.Automation.NeoTaskManager;
using ECommons.Configuration;

namespace AutoWeeklyCap.Runner;

public class Runner
{
    private bool _stopGracefully = false;
    private bool _unlimited = false;
    private bool _leveling = false;

    private State _state = State.Waiting;
    private string? _currentCharacter = null;
    private DateTime _timestamp;

    private int _runsCounter = 0;
    private string? _runsCharacter = null;

    public DateTime? CurrentDutyStartUtc { get; private set; }

    public bool Start()
    {
        if (_state != State.Waiting || _stopGracefully) {
            return false;
        }

        if (!AWC.Config.IsRequiredSettingsSetup()) {
            return false;
        }

        var character = PlayerHelper.GetFullCharacterName();
        if (character == null || !(AWC.Config.GetOrRegisterCharacterOptions(character)?.IsEnabled() ?? false)) {
            StartCharacterSwap();
            return true;
        }

        _currentCharacter = character;
        _timestamp = DateTime.UtcNow;
        _runsCounter = 0;
        _runsCharacter = null;

        _state = CurrencyHelper.IsPlayerLimitedTomestoneCapped()
            ? State.StartingCharacterSwap
            : State.PreparingRunner;

        AWC.Log.Info("Starting weekly cap runner");

        return true;
    }

    public bool AutoStartOnBoot()
    {
        if (_state != State.Waiting || _stopGracefully) {
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
            _state = State.WaitingForAutoRetainer;
            _timestamp = DateTime.UtcNow;
            _runsCounter = 0;
            _runsCharacter = null;
            _currentCharacter = AWC.Config.GetFirstUncappedCharacter();
        });

        return true;
    }

    public void Stop()
    {
        if (AWC.Config.StopRunnerGracefully && PlayerHelper.IsLoggedIn) {
            if (_state is State.RunningAutoDuty or State.SwitchingCharacter || !AutoDutyIPC.IsStopped()) {
                _stopGracefully = true;
                return;
            }
        }

        Abort();
    }

    public void Resume()
    {
        if (!AWC.Config.StopRunnerGracefully || !_stopGracefully || AutoDutyIPC.IsStopped()) {
            return;
        }

        if (_state is not (State.RunningAutoDuty or State.SwitchingCharacter)) {
            return;
        }

        _stopGracefully = false;
    }

    public void Abort()
    {
        _currentCharacter = null;
        _state = State.Waiting;
        _timestamp = DateTime.UtcNow;
        _stopGracefully = false;
        _unlimited = false;
        _leveling = false;
        _runsCounter = 0;
        _runsCharacter = null;
        CurrentDutyStartUtc = null;

        LifestreamIPC.Abort();
        AutoDutyIPC.Stop();
        TitleManager.Reset();
        LocationManager.Reset();
        AWC.TaskManager.Abort();

        AWC.Log.Info("Stopped weekly cap runner");
    }

    public bool IsRunning()
    {
        return _state != State.Waiting;
    }

    public bool IsStopping()
    {
        return _stopGracefully;
    }

    public State GetState()
    {
        return _state;
    }

    public int GetRunsCounter()
    {
        return _runsCounter;
    }

    public string? GetRunsCharacter()
    {
        return _runsCharacter;
    }

    public string? GetCurrentCharacter()
    {
        return _currentCharacter;
    }

    public void Tick()
    {
        if (AWC.TaskManager.IsBusy) {
            return;
        }

        switch (_state) {
            case State.Waiting:
                break;

            case State.PreparingRunner:
                CheckPrerequisitesForRunnerPreparations();
                break;

            case State.WaitingForAutoRetainer:
                WaitForAutoRetainer();
                break;

            case State.CheckingTomestone:
                CheckTomestoneStage();
                break;

            case State.StartingAutoDuty:
                StartAutoDuty();
                break;

            case State.RunningAutoDuty:
                RunAutoDuty();
                break;

            case State.StartingCharacterSwap:
                StartCharacterSwap();
                break;

            case State.SwitchingCharacter:
                SwitchCharacter();
                break;

            case State.StoppingRunner:
                StopRunner();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(_state), _state, null);
        }
    }

    private void CheckPrerequisitesForRunnerPreparations()
    {
        if (_stopGracefully) {
            Abort();
            return;
        }

        if (_currentCharacter == null) {
            AWC.Log.Debug($"Runner: Found no character set for, switching stage");
            AWC.TaskManager.Enqueue(
                () => _state = State.StartingCharacterSwap,
                "next stage: starting character swap"
            );
            return;
        }

        if (AWC.Config.AlwaysStartOnHomeWorld && ActionInstance.Homeworld.Invoke()) {
            return;
        }

        var playerJob = _leveling
            ? LevelingHelper.GetJobToLevel(_currentCharacter)
            : AWC.Config.GetOrRegisterCharacterOptions(_currentCharacter)?.PreferredJob;

        // TODO: Change the job switching from being queued to being called directly, so we're able to invoke the "BuyLevelingUpgrade" action and pause the preparation step if the action is attempting to buy gear upgrades
        using (TitleManager.RegisterTitle(playerJob?.GetIcon() ?? BitmapFontIcon.AnyClass, "Switching Job")) {
            AWC.TaskManager.Enqueue(() => playerJob?.SwitchToJob() ?? true, "switching job");
        }

        if (_leveling && AWC.Config.LevelJobs.BuyExpansionGearUpgrades && ActionInstance.BuyLevelingUpgrade.Invoke()) {
            return;
        }

        if (AWC.Config.AutoRetainerEnabled && AWC.Config.AutoRetainerTrigger.IsWithinThreshold()) {
            _timestamp = DateTime.UtcNow;

            AWC.TaskManager.Enqueue(
                () => _state = State.WaitingForAutoRetainer,
                "next stage: waiting for auto retainer"
            );
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
                                 && _runsCounter == 0;

            var shouldRunForCounter = AWC.Config.DeliverooOnInterval
                                      && _runsCounter % AWC.Config.DeliverooRunInterval == 0
                                      && _runsCounter > 0;

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

        AWC.TaskManager.Enqueue(
            () => _state = State.CheckingTomestone,
            "next stage: checking tomestone"
        );
    }

    private void WaitForAutoRetainer()
    {
        if (!AutoRetainerIPC.GetMultiModeStatus()) {
            AutoRetainerIPC.EnableMultiMode();
        }

        if (AutoRetainerIPC.IsBusy() || LifestreamIPC.IsBusy() || (!PlayerHelper.IsValid && !AddonHelper.IsTitleScreenReady())) {
            _timestamp = DateTime.UtcNow;
            return;
        }

        var elapsed = (DateTime.UtcNow - _timestamp).Seconds;

        switch (PlayerHelper.IsValid) {
            case true when elapsed < 15:
            case false when elapsed < 5:
                return;
        }

        // From this point onwards we're assuming that AutoRetainer has completed its run, next we'll return the original player

        if (_currentCharacter == null) {
            AutoRetainerIPC.DisableMultiMode();

            AWC.TaskManager.Enqueue(
                () => _state = State.StartingCharacterSwap,
                "next stage: starting character swap"
            );
            return;
        }

        if (PlayerHelper.GetFullCharacterName() == _currentCharacter) {
            AWC.TaskManager.Enqueue(
                () => _state = State.PreparingRunner,
                "next stage: preparing runner"
            );
            return;
        }

        var limit = CurrencyHelper.GetLimitedTomestoneWeeklyLimit();
        var tomes = AWC.Config.CollectedTomes.GetValueOrDefault(_currentCharacter, 0);
        if (tomes == limit) {
            AWC.TaskManager.Enqueue(
                () => _state = State.StartingCharacterSwap,
                "next stage: starting character swap"
            );
            return;
        }

        var parts = _currentCharacter.Split("@");

        AWC.Log.Info($"Switching character to {parts[0]} on {parts[1]}");
        _state = State.SwitchingCharacter;
        _timestamp = DateTime.UtcNow;
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

        _timestamp = DateTime.UtcNow;

        if (_unlimited) {
            _state = State.StartingAutoDuty;
            return;
        }

        if (_leveling) {
            var levelableCharacter = LevelingHelper.GetCharacterToLevel();
            if (levelableCharacter == null) {
                Stop();
            } else if (levelableCharacter == _currentCharacter) {
                _state = State.StartingAutoDuty;
            } else {
                _state = State.SwitchingCharacter;
            }

            return;
        }

        _state = isCapped ? State.StartingCharacterSwap : State.StartingAutoDuty;
    }

    private void StartAutoDuty()
    {
        if (_currentCharacter == null) {
            AWC.Log.Debug("Runner: Stopping runner due to character being NULL");
            Stop();
            return;
        }

        if (AWC.Config.OnlyStartAutoDutyFromSafezone) {
            ActionInstance.Safezone.Invoke(_currentCharacter);
        }

        if (_leveling && AWC.Config.LevelJobs.UseStylistForGearUpgrades) {
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
        AWC.TaskManager.Enqueue(() => _timestamp = DateTime.UtcNow, "set timestamp to track timeouts");

        AWC.TaskManager.Enqueue(() =>
            {
                if (!EzThrottler.Throttle("RunnerStartingDuty", 1000)) {
                    return false;
                }

                TitleManager.Reset();

                if (AWC.ClientState.TerritoryType == DutyZone.GetZoneId(_leveling)) {
                    AWC.Log.Debug("Runner: Player detected in the duty zone, switching to RunningAutoDuty stage");
                    _state = State.RunningAutoDuty;

                    CurrentDutyStartUtc ??= DateTime.UtcNow;

                    if (AutoDutyIPC.IsStopped()) {
                        AutoDutyIPC.Run(DutyZone.GetZoneId(_leveling), 1, false);
                    }

                    return true;
                }

                if (!PlayerHelper.IsReady || VNavMeshIPC.IsRunning()) {
                    if (EzThrottler.Throttle("RunnerStartingDutyBusyLog", 2500)) {
                        AWC.Log.Debug($"Runner: Resetting AutoDuty start timer, reason: player is busy or VNavMesh is running");
                    }

                    _timestamp = DateTime.UtcNow;
                    return false;
                }

                unsafe {
                    if ((DateTime.UtcNow - _timestamp).Seconds > 5 && !AutoDutyIPC.IsStopped() && AddonHelper.TryGetReadyAddon("Repair", out _)) {
                        AWC.Log.Debug("Runner: Detected repairing attempt to have been idle for 5+ seconds while trying to start AutoDuty");
                        AWC.Log.Debug("Runner: Stopping AutoDuty and repairing through AWC instead, and then restarting");

                        AutoDutyIPC.Stop();
                        AWC.TaskManager.Abort();
                        ActionInstance.SelfRepair.Invoke();

                        AWC.TaskManager.Enqueue(
                            () => _state = State.CheckingTomestone,
                            "next stage: checking tomestone"
                        );

                        return true;
                    }
                }

                if ((DateTime.UtcNow - _timestamp).Seconds > 30) {
                    AWC.Log.Debug("Runner: Timed out while trying to start AutoDuty");

                    if (_currentCharacter == null) {
                        AWC.Log.Debug("Runner: Stopping runner due to character being NULL");
                        Stop();
                        return true;
                    }

                    AWC.Log.Debug($"Runner: Disabling AWC for {_currentCharacter} and switching character");

                    AWC.Config.Characters[_currentCharacter].Enabled = false;
                    EzConfig.Save();

                    AWC.TaskManager.Enqueue(
                        () => _state = State.StartingCharacterSwap,
                        "next stage: starting character swap"
                    );

                    return true;
                }

                if (EzThrottler.Throttle("RunnerStartingDutyStartAttempt", 1500)) {
                    var zoneId = DutyZone.GetZoneId(_leveling);

                    AWC.Log.Debug(
                        "Runner: Attempting to start AutoDuty: {@Stats}",
                        new Dictionary<string, object> { { "Seconds elapsed", (DateTime.UtcNow - _timestamp).Seconds }, { "AutoDuty started", !AutoDutyIPC.IsStopped() }, { "Current zone", AWC.ClientState.TerritoryType }, { "Duty zone", zoneId } }
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

        if (AWC.ClientState.TerritoryType == DutyZone.GetZoneId(_leveling)) {
            return;
        }

        AWC.Log.Debug("Runner: AutoDuty has complete a run, switching to preparations stage");

        if (AWC.Config.UseBossModRebornAI && BossModRebornIPC.IsEnabled) {
            AWC.Log.Debug("Runner: disabling BossMod Reborn AI");
            ChatHelper.RunCommand("bmrai off");
        }

        if (_leveling && AWC.Config.LevelJobs.UseStylistForGearUpgrades) {
            ActionInstance.EquipGearUpgrade.Invoke();
        }

        if (CurrentDutyStartUtc.HasValue && _currentCharacter != null) {
            var durationSeconds = (int)(DateTime.UtcNow - CurrentDutyStartUtc.Value).TotalSeconds;
            AWC.Log.Debug($"Runner: Finished the run in {durationSeconds} seconds");

            AWC.Config.GetOrRegisterCharacterOptions(_currentCharacter)?.AddDutyDurationSeconds(durationSeconds);
            EzConfig.Save();
        }

        if (_stopGracefully && AWC.Config.NotificationMasterEnabled && AWC.Config.NotificationMasterUsingOnRunnerStopped) {
            ActionInstance.Notification.ForceInvoke(StopNotificationType.RunnerStopped);
        }

        CurrentDutyStartUtc = null;

        if (_runsCharacter != _currentCharacter) {
            _runsCharacter = _currentCharacter;
            _runsCounter = 0;
        }

        _runsCounter++;
        _state = State.PreparingRunner;
    }

    private void StartCharacterSwap()
    {
        var character = AWC.Config.GetFirstUncappedCharacter();
        if (character != null) {
            var parts = character.Split("@");
            if (parts.Length != 2) {
                AWC.Log.Error($"Character {character} is not a valid character name, stopping runner");
                Stop();
                return;
            }

            AWC.Log.Info($"Switching character to {parts[0]} on {parts[1]}");
            _currentCharacter = character;
            _state = State.SwitchingCharacter;
            _timestamp = DateTime.UtcNow;
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        if (_runsCounter > 0 && AWC.Config.NotificationMasterEnabled && AWC.Config.NotificationMasterUsingOnFullyCapped) {
            ActionInstance.Notification.ForceInvoke(StopNotificationType.CharacterCapped);
        }

        if (AWC.Config.StopAction == StopAction.StartUnlimitedRuns) {
            _unlimited = true;
            AWC.Log.Info("All characters have been fully capped, starting unlimited runs");

            var preferredCharacter = AWC.Config.CharacterForSwap;
            if (PlayerHelper.GetFullCharacterName() == preferredCharacter) {
                AWC.Log.Debug("Runner: Player is already on preferred character, starting runner");
                _currentCharacter = preferredCharacter;
                _state = State.PreparingRunner;
                return;
            }

            var parts = preferredCharacter.Split("@");
            if (parts.Length != 2) {
                AWC.Log.Error($"Character {preferredCharacter} is not a valid character name, stopping runner");
                Stop();
                return;
            }

            AWC.Log.Info($"Switching character to {parts[0]} on {parts[1]}");
            _currentCharacter = preferredCharacter;
            _state = State.SwitchingCharacter;
            _timestamp = DateTime.UtcNow;
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        if (AWC.Config.StopAction == StopAction.LevelJobs) {
            _leveling = true;

            var levelableCharacter = LevelingHelper.GetCharacterToLevel();
            if (levelableCharacter == null) {
                AWC.Log.Debug($"Runner: Found no characters to level, stopping runner");
                Stop();
                return;
            }

            if (PlayerHelper.GetFullCharacterName() == levelableCharacter) {
                AWC.Log.Debug("Runner: Player is already on character to level, starting runner");
                _currentCharacter = levelableCharacter;
                _state = State.PreparingRunner;
                return;
            }

            var parts = levelableCharacter.Split("@");
            if (parts.Length != 2) {
                AWC.Log.Error($"Character {levelableCharacter} is not a valid character name, stopping runner");
                Stop();
                return;
            }

            AWC.Log.Info($"Switching character to {parts[0]} on {parts[1]}");
            _currentCharacter = levelableCharacter;
            _state = State.SwitchingCharacter;
            _timestamp = DateTime.UtcNow;
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        AWC.Log.Info("Found no character with missing weekly capped tomestones, stopping runner");
        _state = State.StoppingRunner;
    }

    private void SwitchCharacter()
    {
        if (LifestreamIPC.IsBusy()) {
            return;
        }

        var character = PlayerHelper.GetFullCharacterName();
        if (character == null || character != _currentCharacter) {
            return;
        }

        if (!PlayerHelper.IsReady) {
            return;
        }

        AWC.Log.Info("Completed character swap, preparing runner");
        _state = State.PreparingRunner;
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
