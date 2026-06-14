using AutoWeeklyCap.Contracts.Runner;

namespace AutoWeeklyCap.Runner.Stages;

public class StopRunnerStage : BaseStage
{
    protected override string Name => nameof(StopRunnerStage);

    public override void Handle(Runner runner, RunnerState state)
    {
        runner.Abort();

        AWC.TaskManager.EnqueueDelay(500);
        AWC.TaskManager.Enqueue(
            () => AWC.Config.StopAction.Execute(),
            "executing stop action"
        );
    }
}
