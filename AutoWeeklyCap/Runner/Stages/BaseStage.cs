using AutoWeeklyCap.Runner.Actions;

namespace AutoWeeklyCap.Runner.Stages;

public abstract class BaseStage : QueueableAction
{
    public abstract void Handle(Runner runner, RunnerState state);
}
