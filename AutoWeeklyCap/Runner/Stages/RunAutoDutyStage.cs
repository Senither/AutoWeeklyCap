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
            return;
        }

        if (AWC.ClientState.TerritoryType == DutyZone.GetZoneId(state.LevelingMode)) {
            return;
        }

        AWC.Log.Debug("Runner: AutoDuty has complete a run, switching to preparations stage");

        if (AWC.Config.UseBossModRebornAI && BossModRebornIPC.IsEnabled) {
            AWC.Log.Debug("Runner: disabling BossMod Reborn AI");
            ChatHelper.RunCommand("bmrai off");
        }

        if (state.LevelingMode && AWC.Config.LevelJobs.UseStylistForGearUpgrades) {
            ActionInstance.EquipGearUpgrade.Invoke();
        }

        if (state.CurrentDutyStartUtc.HasValue && state.CurrentCharacter != null) {
            var durationSeconds = (int)(DateTime.UtcNow - state.CurrentDutyStartUtc.Value).TotalSeconds;
            AWC.Log.Debug($"Runner: Finished the run in {durationSeconds} seconds");

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
