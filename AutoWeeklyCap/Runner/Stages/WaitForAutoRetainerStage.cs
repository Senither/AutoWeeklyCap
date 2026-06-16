using AutoWeeklyCap.Contracts.Runner;

namespace AutoWeeklyCap.Runner.Stages;

public class WaitForAutoRetainerStage : BaseStage
{
    protected override string Name => nameof(WaitForAutoRetainerStage);

    public override void Handle(Runner runner, RunnerState state)
    {
        if (!AutoRetainerIPC.GetMultiModeStatus()) {
            AutoRetainerIPC.EnableMultiMode();
        }

        if (AutoRetainerIPC.IsBusy() || LifestreamIPC.IsBusy() || (!PlayerHelper.IsValid && !AddonHelper.IsTitleScreenReady())) {
            state.UpdateTimestamp();
            return;
        }

        var elapsed = (DateTime.UtcNow - state.Timestamp).Seconds;

        switch (PlayerHelper.IsValid) {
            case true when elapsed < 15:
            case false when elapsed < 5:
                return;
        }

        // From this point onwards we're assuming that AutoRetainer has completed its run, next we'll return the original player

        if (state.CurrentCharacter == null) {
            AutoRetainerIPC.DisableMultiMode();
            state.ChangeStageTo(Stage.StartingCharacterSwap);
            return;
        }

        if (PlayerHelper.GetFullCharacterName() == state.CurrentCharacter) {
            state.ChangeStageTo(Stage.PreparingRunner);
            return;
        }

        var limit = CurrencyHelper.GetLimitedTomestoneWeeklyLimit();
        var tomes = AWC.Config.CollectedTomes.GetValueOrDefault(state.CurrentCharacter, 0);
        if (tomes == limit) {
            state.ChangeStageTo(Stage.StartingCharacterSwap);
            return;
        }

        var parts = state.CurrentCharacter.Split("@");

        LogInfo($"Switching character to {parts[0]} on {parts[1]}");
        state.ChangeStageTo(Stage.SwitchingCharacter);
        state.UpdateTimestamp();
        LifestreamIPC.ChangeCharacter(parts[0], parts[1]);
    }
}
