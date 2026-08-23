using AutoWeeklyCap.Config;
using AutoWeeklyCap.Contracts.Runner;
using AutoWeeklyCap.IPC.AutoDuty;
using AutoWeeklyCap.Runner.Zone;

using ECommons.Automation.NeoTaskManager;

namespace AutoWeeklyCap.Runner.Stages;

public class StartAutoDutyStage : BaseStage
{
    protected override string Name => nameof(StartAutoDutyStage);

    public override void Handle(Runner runner, RunnerState state)
    {
        if (state.CurrentCharacter == null) {
            LogDebug("Stopping runner due to character being NULL");
            runner.Stop();
            return;
        }

        state.DisableSkipPlayerJobSwitch();

        if (state.LevelingMode && AWC.Config.LevelJobs.UseLevelingFood) {
            ActionInstance.UseFood.Invoke();
        }

        if (AWC.Config.OnlyStartAutoDutyFromSafezone) {
            ActionInstance.Safezone.Invoke(state.CurrentCharacter);
        }

        if (state.LevelingMode && AWC.Config.LevelJobs.UseStylistForGearUpgrades) {
            ActionInstance.EquipGearUpgrade.Invoke();
        }

        AWC.TaskManager.Enqueue(LocationManager.RegisterLocation, "register last known location");
        AWC.TaskManager.Enqueue(state.UpdateTimestamp, "set timestamp to track timeouts");

        AWC.TaskManager.Enqueue(
            () => StartAutoDuty(runner, state),
            "starting AutoDuty",
            new TaskManagerConfiguration(120_000) // 2 minutes
        );
    }

    private bool StartAutoDuty(Runner runner, RunnerState state)
    {
        if (!EzThrottler.Throttle("RunnerStartingDuty", 1000)) {
            return false;
        }

        TitleManager.Reset();

        uint zoneId = DutyZone.GetZoneId(state.LevelingMode);
        if (AWC.ClientState.TerritoryType == zoneId) {
            LogDebug("Player detected in the duty zone, switching to RunningAutoDuty stage");

            state.ChangeStageTo(Stage.RunningAutoDuty);
            state.UpsertCurrentDutyStartUtc(DateTime.UtcNow);

            state.SetMetric(Constants.MetricUncappedAcquiredTomestoneKey, (uint)CurrencyHelper.GetUncappedAcquiredTomestoneCount());
            state.SetMetric(Constants.MetricWeeklyAcquiredLimitedTomestoneKey, (uint)CurrencyHelper.GetWeeklyAcquiredLimitedTomestoneCount());

            if (AutoDutyIPC.IsStopped()) {
                AWC.Log.Debug("Attempting to resume AutoDuty while already in zone");

                ApplyAutoDutyProfile();
                AutoDutyIPC.Start(false);
            }

            return true;
        }

        if (!PlayerHelper.IsReady || VNavMeshIPC.IsRunning()) {
            if (EzThrottler.Throttle("RunnerStartingDutyBusyLog", 2500)) {
                LogDebug($"Resetting AutoDuty start timer, reason: player is busy or VNavMesh is running");
            }

            state.UpdateTimestamp();
            return false;
        }

        unsafe {
            if ((DateTime.UtcNow - state.Timestamp).Seconds > 5 && !AutoDutyIPC.IsStopped() && AddonHelper.TryGetReadyAddon("Repair", out _)) {
                LogDebug("Detected repairing attempt to have been idle for 5+ seconds while trying to start AutoDuty");
                LogDebug("Stopping AutoDuty and repairing through AWC instead, and then restarting");

                AutoDutyIPC.Stop();
                AWC.TaskManager.Abort();
                ActionInstance.SelfRepair.Invoke();

                state.ChangeStageTo(Stage.CheckingTomestone);

                return true;
            }
        }

        if ((DateTime.UtcNow - state.Timestamp).Seconds > 30) {
            LogDebug("Timed out while trying to start AutoDuty");

            if (state.CurrentCharacter == null) {
                LogDebug("Stopping runner due to character being NULL");
                runner.Stop();
                return true;
            }

            LogDebug($"Disabling AWC for {state.CurrentCharacter} and switching character");

            AWC.Config.Characters[state.CurrentCharacter].Enabled = false;
            Configuration.Save();

            state.ChangeStageTo(Stage.StartingCharacterSwap);

            return true;
        }

        if (!EzThrottler.Throttle("RunnerStartingDutyStartAttempt", 1500)) {
            return false;
        }

        LogDebug(
            "Attempting to start AutoDuty: {@Stats}",
            new Dictionary<string, object> { { "Seconds elapsed", (DateTime.UtcNow - state.Timestamp).Seconds }, { "AutoDuty started", !AutoDutyIPC.IsStopped() }, { "Current zone", AWC.ClientState.TerritoryType }, { "Duty zone", zoneId } }
        );

        if (zoneId == 0) {
            LogDebug("Territory Type ID was detected as zero (0), stopping runner");
            runner.Stop();
            return true;
        }

        ApplyAutoDutyProfile();

        AutoDutyIPC.Run(zoneId, 1, false);

        return false;
    }

    private void ApplyAutoDutyProfile()
    {
        if (AWC.Config.UseAutoDutyProfileOverride) {
            AutoDutyProfile.Apply();
        }
    }
}
