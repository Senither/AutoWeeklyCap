using AutoWeeklyCap.Contracts.Runner;

namespace AutoWeeklyCap.Runner.Stages;

public class WaitingStage : BaseStage
{
    protected override string Name => nameof(WaitingStage);

    public override void Handle(Runner runner, RunnerState state)
    {
        // This does nothing, it's just waiting until
        // the runner actually does something
    }
}
