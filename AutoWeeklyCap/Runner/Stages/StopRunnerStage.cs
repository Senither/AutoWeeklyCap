namespace AutoWeeklyCap.Runner.Stages;

public class StopRunnerStage : BaseStage
{
    protected override string Name => nameof(StopRunnerStage);

    public override void Handle(RunnerState state)
    {
        // TODO: call abort
        // Abort();

        AWC.TaskManager.EnqueueDelay(500);
        AWC.TaskManager.Enqueue(
            () => AWC.Config.StopAction.Execute(),
            "executing stop action"
        );
    }
}
