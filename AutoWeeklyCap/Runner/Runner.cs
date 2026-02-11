using AutoWeeklyCap.Actions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.ImGuiNotification;
using ECommons.Automation.NeoTaskManager;

namespace AutoWeeklyCap.Runner;

public class Runner
{
    private bool stopGracefully = false;
    private bool unlimited = false;

    private State state = State.Waiting;
    private string? currentCharacter = null;
    private DateTime timestamp;

    private int runsCounter = 0;
    private string? runsCharacter = null;

    public DateTime? CurrentDutyStartUtc { get; private set; }

    public bool Start()
    {
        if (state != State.Waiting || stopGracefully)
            return false;

        var zoneName = MapHelper.GetZoneNameFromId(AWC.Config.ZoneId);
        if (zoneName == null)
            return false;

        var character = PlayerHelper.GetFullCharacterName();
        if (character == null || !AWC.Config.GetOrRegisterCharacterOptions(character).IsEnabled())
        {
            StartCharacterSwap();
            return true;
        }

        currentCharacter = character;
        state = State.PreparingRunner;
        timestamp = DateTime.UtcNow;
        runsCounter = 0;
        runsCharacter = null;

        AWC.Log.Debug("Starting weekly cap runner");

        return true;
    }

    public bool AutoStartOnBoot()
    {
        if (state != State.Waiting || stopGracefully)
            return false;

        var zoneName = MapHelper.GetZoneNameFromId(AWC.Config.ZoneId);
        if (zoneName == null)
            return false;

        if (PlayerHelper.IsValid)
            return false;

        if (AWC.Config.GetFirstUncappedCharacter() == null)
            return false;

        const int autoStartDelay = 5;
        for (var i = 0; i < autoStartDelay; i++)
        {
            var seconds = autoStartDelay - i;
            AWC.TaskManager.Enqueue(() => Svc.NotificationManager.AddNotification(new Notification
            {
                Content = $"Auto start AWC in {seconds}!",
                InitialDuration = TimeSpan.FromSeconds(1),
                HardExpiry = DateTime.Now.AddSeconds(1),
                Type = NotificationType.Warning,
            }));

            AWC.TaskManager.EnqueueDelay(1000);
        }

        AWC.TaskManager.Enqueue(() =>
        {
            state = State.WaitingForAutoRetainer;
            timestamp = DateTime.UtcNow;
            runsCounter = 0;
            runsCharacter = null;
        });

        return true;
    }

    public void Stop()
    {
        if (AWC.Config.StopRunnerGracefully && PlayerHelper.IsLoggedIn)
        {
            if (state is State.RunningAutoDuty or State.SwitchingCharacter || !AutoDutyIPC.IsStopped())
            {
                stopGracefully = true;
                return;
            }
        }

        Abort();
    }

    public void Resume()
    {
        if (!AWC.Config.StopRunnerGracefully || !stopGracefully || AutoDutyIPC.IsStopped())
            return;

        if (state is not (State.RunningAutoDuty or State.SwitchingCharacter))
            return;

        stopGracefully = false;
    }

    public void Abort()
    {
        currentCharacter = null;
        state = State.Waiting;
        timestamp = DateTime.UtcNow;
        stopGracefully = false;
        unlimited = false;
        runsCounter = 0;
        runsCharacter = null;
        CurrentDutyStartUtc = null;

        LifestreamIPC.Abort();
        AutoDutyIPC.Stop();
        AWC.TaskManager.Abort();

        AWC.Log.Debug("Stopped weekly cap runner");
    }

    public bool IsRunning() => state != State.Waiting;
    public bool IsStopping() => stopGracefully;

    public string GetStatus() => state.GetStatus(stopGracefully, currentCharacter);
    public string GetStatusShort() => state.GetStatusShort(stopGracefully, currentCharacter);
    public BitmapFontIcon GetStatusIcon() => state.GetStatusIcon(stopGracefully);

    public int GetRunsCounter() => runsCounter;
    public string? GetRunsCharacter() => runsCharacter;

    public void Tick()
    {
        if (AWC.TaskManager.IsBusy)
            return;

        switch (state)
        {
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
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void CheckPrerequisitesForRunnerPreparations()
    {
        if (stopGracefully)
        {
            Abort();
            return;
        }

        if (currentCharacter == null)
        {
            AWC.Log.Debug("Stopping runner due to character being NULL");
            Stop();
            return;
        }

        if (AWC.Config.AutoRetainerEnabled && AutoRetainerHelper.HasRetainerWithinThreshold())
        {
            timestamp = DateTime.UtcNow;

            AWC.TaskManager.Enqueue(
                () => state = State.WaitingForAutoRetainer,
                "next stage: waiting for auto retainer"
            );
            return;
        }

        AWC.TaskManager.Enqueue(() =>
        {
            if (AutoRetainerIPC.IsEnabled && AutoRetainerIPC.GetMultiModeStatus())
            {
                if (!AutoRetainerIPC.IsBusy())
                    AutoRetainerIPC.DisableMultiMode();

                return false;
            }

            return true;
        }, "disable AutoRetainer multi mode when it's not busy");

        if (AWC.Config.Extract)
            ActionInstance.Extract.Invoke();

        if (AWC.Config.Repair && InventoryHelper.CanRepair(AWC.Config.RepairPercentage))
        {
            if (AWC.Config.RepairSelf)
                ActionInstance.SelfRepair.Invoke();
            else
                ActionInstance.NpcRepair.Invoke();
        }

        if (AWC.Config.DeliverooEnabled)
        {
            var shouldRunFirst = AWC.Config.DeliverooRunOnFirstLoop
                                 && runsCounter == 0;

            var shouldRunForCounter = AWC.Config.DeliverooOnInterval
                                      && runsCounter % AWC.Config.DeliverooRunInterval == 0
                                      && runsCounter > 0;

            AWC.Log.Debug($"Deliveroo check [first: {shouldRunFirst}, forCounter: {shouldRunForCounter}]");
            if (shouldRunFirst || shouldRunForCounter)
                ActionInstance.Deliveroo.Invoke();
        }

        if (AWC.Config.SpendUncappedTomestones)
        {
            if (CurrencyHelper.GetUncappedAcquiredTomestoneCount() >= AWC.Config.SpendUncappedTomestoneThreshold)
                ActionInstance.SpendTomestone.Invoke();
        }

        AWC.TaskManager.Enqueue(
            () => state = State.CheckingTomestone,
            "next stage: checking tomestone"
        );
    }

    private void WaitForAutoRetainer()
    {
        if (!AutoRetainerIPC.GetMultiModeStatus())
            AutoRetainerIPC.EnableMultiMode();

        if (AutoRetainerIPC.IsBusy() || LifestreamIPC.IsBusy() || (!PlayerHelper.IsValid && !AddonHelper.IsTitleScreenReady()))
        {
            timestamp = DateTime.UtcNow;
            return;
        }

        var elapsed = (DateTime.UtcNow - timestamp).Seconds;

        switch (PlayerHelper.IsValid)
        {
            case true when elapsed < 15:
            case false when elapsed < 5:
                return;
        }

        // From this point onwards we're assuming that AutoRetainer has completed its run, next we'll return the original player

        if (currentCharacter == null)
        {
            AutoRetainerIPC.DisableMultiMode();

            AWC.TaskManager.Enqueue(
                () => state = State.StartingCharacterSwap,
                "next stage: starting character swap"
            );
            return;
        }

        if (PlayerHelper.GetFullCharacterName() == currentCharacter)
        {
            AWC.TaskManager.Enqueue(
                () => state = State.PreparingRunner,
                "next stage: checking tomestone"
            );
            return;
        }

        var limit = CurrencyHelper.GetLimitedTomestoneWeeklyLimit();
        var tomes = AWC.Config.CollectedTomes.GetValueOrDefault(currentCharacter, 0);
        if (tomes == limit)
        {
            AWC.TaskManager.Enqueue(
                () => state = State.StartingCharacterSwap,
                "next stage: starting character swap"
            );
            return;
        }

        var parts = currentCharacter.Split("@");

        AWC.Log.Debug($"Switching character to {parts[0]} on {parts[1]}");
        state = State.SwitchingCharacter;
        timestamp = DateTime.UtcNow;
        LifestreamIPC.ChangeCharacter(parts[0], parts[1]);
    }

    private void CheckTomestoneStage()
    {
        var character = PlayerHelper.GetFullCharacterName();
        if (character == null)
            return;

        var isCapped = CurrencyHelper.GetLimitedTomestoneWeeklyLimit() == CurrencyHelper.GetWeeklyAcquiredTomestoneCount();
        if (isCapped && AWC.Config.DeliverooEnabled && !AWC.Config.DeliverooOnInterval)
            ActionInstance.Deliveroo.Invoke();

        timestamp = DateTime.UtcNow;

        if (unlimited)
        {
            state = State.StartingAutoDuty;
            return;
        }

        state = isCapped ? State.StartingCharacterSwap : State.StartingAutoDuty;
    }

    private void StartAutoDuty()
    {
        if (currentCharacter == null)
        {
            AWC.Log.Debug("Stopping runner due to character being NULL");
            Stop();
            return;
        }

        AWC.TaskManager.Enqueue(
            () => AWC.Config.GetOrRegisterCharacterOptions(currentCharacter).PreferredJob.SwitchToJob(),
            "switch to preferred job"
        );

        AWC.TaskManager.Enqueue(() =>
        {
            if (AWC.Config.UseBossModRebornAI && BossModRebornIPC.IsEnabled)
            {
                AWC.Log.Debug("UseBossModRebornAI is enabled and BossMod Reborn is disabled, enabling AI");
                ChatHelper.RunCommand("bmrai on");
            }
        }, "enable BossMod Reborn AI if option is enabled");

        AWC.TaskManager.Enqueue(
            () =>
            {
                if (!EzThrottler.Throttle("RunnerStartingDuty", 1000))
                    return false;

                if (AWC.ClientState.TerritoryType == AWC.Config.ZoneId)
                {
                    AWC.Log.Debug("Player detected in the duty zone, switching to RunningAutoDuty stage");
                    state = State.RunningAutoDuty;

                    CurrentDutyStartUtc ??= DateTime.UtcNow;

                    if (AutoDutyIPC.IsStopped())
                        AutoDutyIPC.Run(AWC.Config.ZoneId, 1, false);

                    return true;
                }

                if (!PlayerHelper.IsReady || VNavMeshIPC.IsRunning())
                {
                    if (EzThrottler.Throttle("RunnerStartingDutyBusyLog", 2500))
                        AWC.Log.Debug($"Resetting AutoDuty start timer, reason: player is busy or VNavMesh is running");

                    timestamp = DateTime.UtcNow;
                    return false;
                }

                unsafe
                {
                    if ((DateTime.UtcNow - timestamp).Seconds > 5 && !AutoDutyIPC.IsStopped() && AddonHelper.TryGetReadyAddon("Repair", out _))
                    {
                        AWC.Log.Debug("Detected repairing attempt to have been idle for 5+ seconds while trying to start AutoDuty");
                        AWC.Log.Debug("Stopping AutoDuty and repairing through AWC instead, and then restarting");

                        AutoDutyIPC.Stop();
                        AWC.TaskManager.Abort();
                        ActionInstance.SelfRepair.Invoke();

                        AWC.TaskManager.Enqueue(
                            () => state = State.CheckingTomestone,
                            "next stage: checking tomestone"
                        );

                        return true;
                    }
                }

                if ((DateTime.UtcNow - timestamp).Seconds > 30)
                {
                    AWC.Log.Debug("Timed out while trying to start AutoDuty");

                    if (currentCharacter == null)
                    {
                        AWC.Log.Debug("Stopping runner due to character being NULL");
                        Stop();
                        return true;
                    }

                    AWC.Log.Debug($"Disabling AWC for {currentCharacter} and switching character");

                    AWC.Config.Characters[currentCharacter].Enabled = false;
                    AWC.Config.Save();

                    AWC.TaskManager.Enqueue(
                        () => state = State.StartingCharacterSwap,
                        "next stage: starting character swap"
                    );

                    return true;
                }

                if (EzThrottler.Throttle("RunnerStartingDutyStartAttempt", 1500))
                {
                    AWC.Log.Debug("Attempting to start AutoDuty: {@Stats}", new Dictionary<string, object>
                    {
                        { "Seconds elapsed", (DateTime.UtcNow - timestamp).Seconds },
                        { "AutoDuty started", !AutoDutyIPC.IsStopped() },
                        { "Current zone", AWC.ClientState.TerritoryType },
                        { "Duty zone", AWC.Config.ZoneId },
                    });

                    AutoDutyIPC.Run(AWC.Config.ZoneId, 1, false);
                }

                return false;
            },
            "starting AutoDuty",
            new TaskManagerConfiguration(timeLimitMS: 120_000) // 2 minutes
        );
    }

    private void RunAutoDuty()
    {
        if (!AutoDutyIPC.IsStopped())
            return;

        if (AWC.ClientState.TerritoryType == AWC.Config.ZoneId)
            return;

        AWC.Log.Debug("AutoDuty has complete a run, switching to preparations stage");

        if (CurrentDutyStartUtc.HasValue && currentCharacter != null)
        {
            var durationSeconds = (int)(DateTime.UtcNow - CurrentDutyStartUtc.Value).TotalSeconds;
            AWC.Log.Debug($"Finished the run in {durationSeconds} seconds");

            AWC.Config.GetOrRegisterCharacterOptions(currentCharacter).AddDutyDurationSeconds(durationSeconds);
            AWC.Config.Save();
        }

        if (stopGracefully && AWC.Config.NotificationMasterEnabled && AWC.Config.NotificationMasterUsingOnRunnerStopped)
            ActionInstance.Notification.ForceInvoke(Actions.NotificationType.RunnerStopped);

        CurrentDutyStartUtc = null;

        runsCharacter ??= currentCharacter;
        if (runsCharacter != currentCharacter)
        {
            runsCharacter = currentCharacter;
            runsCounter = 0;
        }

        runsCounter++;
        state = State.PreparingRunner;
    }

    private void StartCharacterSwap()
    {
        var character = AWC.Config.GetFirstUncappedCharacter();
        if (character != null)
        {
            var parts = character.Split("@");
            if (parts.Length != 2)
            {
                AWC.Log.Error($"Character {character} is not a valid character name, stopping runner");
                Stop();
                return;
            }

            AWC.Log.Debug($"Switching character to {parts[0]} on {parts[1]}");
            currentCharacter = character;
            state = State.SwitchingCharacter;
            timestamp = DateTime.UtcNow;
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        if (AWC.Config.NotificationMasterEnabled && AWC.Config.NotificationMasterUsingOnFullyCapped)
            ActionInstance.Notification.ForceInvoke(Actions.NotificationType.CharacterCapped);

        if (AWC.Config.StopAction == StopAction.StartUnlimitedRuns)
        {
            unlimited = true;
            AWC.Log.Debug("All characters have been fully capped, starting unlimited runs");

            var preferredCharacter = AWC.Config.CharacterForSwap;
            if (PlayerHelper.GetFullCharacterName() == preferredCharacter)
            {
                AWC.Log.Debug("Player is already on preferred character, starting AutoDuty");
                state = State.StartingAutoDuty;
                return;
            }

            var parts = preferredCharacter.Split("@");
            if (parts.Length != 2)
            {
                AWC.Log.Error($"Character {preferredCharacter} is not a valid character name, stopping runner");
                Stop();
                return;
            }

            AWC.Log.Debug($"Switching character to {parts[0]} on {parts[1]}");
            currentCharacter = preferredCharacter;
            state = State.SwitchingCharacter;
            timestamp = DateTime.UtcNow;
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        AWC.Log.Debug("Found no character with missing weekly capped tomestones, stopping runner");
        state = State.StoppingRunner;
    }

    private void SwitchCharacter()
    {
        if (LifestreamIPC.IsBusy())
            return;

        var character = PlayerHelper.GetFullCharacterName();
        if (character == null || character != currentCharacter)
            return;

        AWC.Log.Debug("Completed character swap, checking tomestones");
        state = State.PreparingRunner;
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
