using AutoWeeklyCap.Contracts.Runner;

namespace AutoWeeklyCap.Runner.Stages;

public class PreparingRunnerStage : BaseStage
{
    protected override string Name => nameof(PreparingRunnerStage);

    public override void Handle(Runner runner, RunnerState state)
    {
        if (state.StoppingGracefully) {
            runner.Abort();
            return;
        }

        if (state.CurrentCharacter == null) {
            LogDebug("Found no character set for, switching stage");
            state.ChangeStageTo(Stage.StartingCharacterSwap);
            return;
        }

        if (AWC.Config.AlwaysStartOnHomeWorld && ActionInstance.Homeworld.Invoke()) {
            return;
        }

        PlayerJob? playerJob = state.LevelingMode
            ? LevelingHelper.GetJobToLevel(state.CurrentCharacter)
            : AWC.Config.GetOrRegisterCharacterOptions(state.CurrentCharacter)?.PreferredJob;

        using (TitleManager.RegisterTitle(playerJob?.GetIcon() ?? BitmapFontIcon.AnyClass, "Switching Job")) {
            if (!playerJob?.IsAlreadyOnJob() ?? false) {
                AWC.TaskManager.Enqueue(() => playerJob?.SwitchToJob() ?? true, "switching job");
                return;
            }
        }

        if (state.LevelingMode && AWC.Config.LevelJobs.BuyExpansionGearUpgrades && ActionInstance.BuyLevelingUpgrade.Invoke()) {
            return;
        }

        if (AWC.Config.AutoRetainerEnabled && AWC.Config.AutoRetainerTrigger.IsWithinThreshold()) {
            state.UpdateTimestamp();
            state.ChangeStageTo(Stage.WaitingForAutoRetainer);
            return;
        }

        AWC.TaskManager.Enqueue(() =>
        {
            if (!AutoRetainerIPC.IsEnabled || !AutoRetainerIPC.GetMultiModeStatus()) {
                return true;
            }

            if (!AutoRetainerIPC.IsBusy()) {
                AutoRetainerIPC.DisableMultiMode();
            }

            return false;
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

        if (AWC.Config.DeliverooEnabled && ShouldRunDeliveroo(state)) {
            ActionInstance.EnqueueAction(ActionInstance.Deliveroo);
        }

        if (AWC.Config.SpendUncappedTomestones) {
            if (CurrencyHelper.GetUncappedAcquiredTomestoneCount() >= AWC.Config.SpendUncappedTomestoneThreshold) {
                ActionInstance.EnqueueAction(ActionInstance.SpendTomestone);
            }
        }

        state.ChangeStageTo(Stage.CheckingTomestone);
    }

    private bool ShouldRunDeliveroo(RunnerState state)
    {
        bool shouldRunFirst = AWC.Config.DeliverooRunOnFirstLoop &&
                              state.RunsCounter == 0;

        bool shouldRunForCounter = AWC.Config.DeliverooOnInterval &&
                                   state.RunsCounter % AWC.Config.DeliverooRunInterval == 0 &&
                                   state.RunsCounter > 0;

        LogDebug($"Deliveroo check [first: {shouldRunFirst}, forCounter: {shouldRunForCounter}]");

        return shouldRunFirst || shouldRunForCounter;
    }
}
