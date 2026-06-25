using AutoWeeklyCap.Contracts.Runner;

namespace AutoWeeklyCap.Runner.Stages;

public class CheckTomestoneStage : BaseStage
{
    protected override string Name => nameof(CheckTomestoneStage);

    public override void Handle(Runner runner, RunnerState state)
    {
        string? character = PlayerHelper.GetFullCharacterName();
        if (character == null) {
            return;
        }

        bool isCapped = CurrencyHelper.IsPlayerLimitedTomestoneCapped();
        if (isCapped && AWC.Config.DeliverooEnabled && !AWC.Config.DeliverooOnInterval) {
            ActionInstance.Deliveroo.Invoke();
        }

        state.UpdateTimestamp();

        if (state.UnlimitedMode) {
            state.ChangeStageTo(Stage.StartingAutoDuty);
            return;
        }

        if (state.LevelingMode) {
            var levelableCharacter = LevelingHelper.GetCharacterToLevel();
            if (levelableCharacter == null) {
                runner.Stop();
            } else if (levelableCharacter == state.CurrentCharacter) {
                state.ChangeStageTo(Stage.StartingAutoDuty);
            } else {
                state.ChangeStageTo(Stage.StartingCharacterSwap);
            }

            return;
        }

        state.ChangeStageTo(
            isCapped ? Stage.StartingCharacterSwap : Stage.StartingAutoDuty
        );
    }
}
