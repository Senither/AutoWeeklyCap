namespace AutoWeeklyCap.Contracts.Runner;

public abstract class BaseStage : QueueableNamedTasks
{
    public abstract void Handle(global::AutoWeeklyCap.Runner.Runner runner, RunnerState state);
}
