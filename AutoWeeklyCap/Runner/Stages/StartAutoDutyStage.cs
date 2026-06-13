using AutoWeeklyCap.Runner.Zone;

using ECommons.Automation.NeoTaskManager;
using ECommons.Configuration;

namespace AutoWeeklyCap.Runner.Stages;

public class StartAutoDutyStage : BaseStage
{
    protected override string Name => nameof(StartAutoDutyStage);

    public override void Handle(RunnerState state)
    {
        if (state.CurrentCharacter == null) {
            AWC.Log.Debug("Runner: Stopping runner due to character being NULL");
            // TODO: call stop
            // Stop();
            return;
        }

        if (AWC.Config.OnlyStartAutoDutyFromSafezone) {
            ActionInstance.Safezone.Invoke(state.CurrentCharacter);
        }

        if (state.LevelingMode && AWC.Config.LevelJobs.UseStylistForGearUpgrades) {
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
        AWC.TaskManager.Enqueue(() => state.UpdateTimestamp(), "set timestamp to track timeouts");

        AWC.TaskManager.Enqueue(() =>
            {
                if (!EzThrottler.Throttle("RunnerStartingDuty", 1000)) {
                    return false;
                }

                TitleManager.Reset();

                if (AWC.ClientState.TerritoryType == DutyZone.GetZoneId(state.LevelingMode)) {
                    AWC.Log.Debug("Runner: Player detected in the duty zone, switching to RunningAutoDuty stage");
                    state.ChangeStageTo(Stage.RunningAutoDuty);
                    state.UpsertCurrentDutyStartUtc(DateTime.UtcNow);

                    if (AutoDutyIPC.IsStopped()) {
                        AutoDutyIPC.Run(DutyZone.GetZoneId(state.LevelingMode), 1, false);
                    }

                    return true;
                }

                if (!PlayerHelper.IsReady || VNavMeshIPC.IsRunning()) {
                    if (EzThrottler.Throttle("RunnerStartingDutyBusyLog", 2500)) {
                        AWC.Log.Debug($"Runner: Resetting AutoDuty start timer, reason: player is busy or VNavMesh is running");
                    }

                    state.UpdateTimestamp();
                    return false;
                }

                unsafe {
                    if ((DateTime.UtcNow - state.Timestamp).Seconds > 5 && !AutoDutyIPC.IsStopped() && AddonHelper.TryGetReadyAddon("Repair", out _)) {
                        AWC.Log.Debug("Runner: Detected repairing attempt to have been idle for 5+ seconds while trying to start AutoDuty");
                        AWC.Log.Debug("Runner: Stopping AutoDuty and repairing through AWC instead, and then restarting");

                        AutoDutyIPC.Stop();
                        AWC.TaskManager.Abort();
                        ActionInstance.SelfRepair.Invoke();

                        state.ChangeStageTo(Stage.CheckingTomestone);

                        return true;
                    }
                }

                if ((DateTime.UtcNow - state.Timestamp).Seconds > 30) {
                    AWC.Log.Debug("Runner: Timed out while trying to start AutoDuty");

                    if (state.CurrentCharacter == null) {
                        AWC.Log.Debug("Runner: Stopping runner due to character being NULL");
                        // TODO: call stop
                        // Stop();
                        return true;
                    }

                    AWC.Log.Debug($"Runner: Disabling AWC for {state.CurrentCharacter} and switching character");

                    AWC.Config.Characters[state.CurrentCharacter].Enabled = false;
                    EzConfig.Save();

                    state.ChangeStageTo(Stage.StartingCharacterSwap);

                    return true;
                }

                if (EzThrottler.Throttle("RunnerStartingDutyStartAttempt", 1500)) {
                    var zoneId = DutyZone.GetZoneId(state.LevelingMode);

                    AWC.Log.Debug(
                        "Runner: Attempting to start AutoDuty: {@Stats}",
                        new Dictionary<string, object> { { "Seconds elapsed", (DateTime.UtcNow - state.Timestamp).Seconds }, { "AutoDuty started", !AutoDutyIPC.IsStopped() }, { "Current zone", AWC.ClientState.TerritoryType }, { "Duty zone", zoneId } }
                    );

                    if (zoneId == 0) {
                        AWC.Log.Debug("Runner: Territory Type ID was detected as zero (0), stopping runner");
                        // TODO: call stop
                        // Stop();
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
}
