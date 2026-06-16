using AutoWeeklyCap.Contracts.Runner;

namespace AutoWeeklyCap.Runner.Stages;

public class SwitchingCharacterStage : BaseStage
{
    protected override string Name => nameof(SwitchingCharacterStage);

    public override void Handle(Runner runner, RunnerState state)
    {
        if (LifestreamIPC.IsBusy()) {
            return;
        }

        var character = PlayerHelper.GetFullCharacterName();
        if (character == null || character != state.CurrentCharacter) {
            return;
        }

        if (!PlayerHelper.IsReady) {
            return;
        }

        LogInfo("Completed character swap, preparing runner");
        state.ChangeStageTo(Stage.PreparingRunner);
    }
}
