using AutoWeeklyCap.Contracts.Runner;
using AutoWeeklyCap.Runner.Zone;

using ECommons.Configuration;

namespace AutoWeeklyCap.Runner.Stages;

public class RunAutoDutyStage : BaseStage
{
    protected override string Name => nameof(StartAutoDutyStage);

    public override void Handle(Runner runner, RunnerState state)
    {
        if (!AutoDutyIPC.IsStopped()) {
            HandleAiState(state);
            return;
        }

        if (AWC.ClientState.TerritoryType == DutyZone.GetZoneId(state.LevelingMode)) {
            return;
        }

        HandleCompletedRun(state);
    }

    private void HandleAiState(RunnerState state)
    {
        if (state.UsingBossModRebornAi) {
            return;
        }

        if (!EzThrottler.Throttle("HandleAiState", 2500)) {
            return;
        }

        if (!AWC.Config.UseBossModRebornAI || !BossModRebornIPC.IsEnabled) {
            return;
        }

        if (!PlayerHelper.InCombat) {
            return;
        }

        LogDebug("enabling BossMod Reborn AI");
        BossModRebornIPC.EnableAI();
        state.SetUsingBossModRebornAi(true);
    }

    private void HandleCompletedRun(RunnerState state)
    {
        LogDebug("AutoDuty has complete a run, switching to preparations stage");

        if (AWC.Config.UseBossModRebornAI && BossModRebornIPC.IsEnabled) {
            LogDebug("disabling BossMod Reborn AI");
            BossModRebornIPC.DisableAI();
            state.SetUsingBossModRebornAi(false);
        }

        if (state.LevelingMode && AWC.Config.LevelJobs.UseStylistForGearUpgrades) {
            ActionInstance.EquipGearUpgrade.Invoke();
        }

        if (state.CurrentDutyStartUtc.HasValue && state.CurrentCharacter != null) {
            int durationSeconds = (int)(DateTime.UtcNow - state.CurrentDutyStartUtc.Value).TotalSeconds;
            LogDebug($"Finished the run in {durationSeconds} seconds");

            if (state.IsInNormalMode()) {
                AWC.Config.GetOrRegisterCharacterOptions(state.CurrentCharacter)?.AddDutyDurationSeconds(durationSeconds);
                EzConfig.Save();
            }
        }

        if (state.StoppingGracefully && AWC.Config.NotificationMasterEnabled && AWC.Config.NotificationMasterUsingOnRunnerStopped) {
            ActionInstance.Notification.ForceInvoke(
                state.LevelingMode
                    ? StopNotificationType.LevelingRunStopped
                    : StopNotificationType.RunnerStopped
            );
        }

        state.SetCurrentDutyStartUtc(null);

        if (state.RunsCharacter != state.CurrentCharacter) {
            state.SetRunsCharacter(state.CurrentCharacter);
            state.SetRunsCounter(0);
        }

        state.IncrementRunsCounter();
        state.ChangeStageTo(Stage.PreparingRunner);
    }
}
