namespace AutoWeeklyCap.Contracts.Runner;

public abstract class BaseStage : QueueableAction
{
    public abstract void Handle(global::AutoWeeklyCap.Runner.Runner runner, RunnerState state);

    protected void LogDebug(string message)
    {
        AWC.Log.Debug($"{Name}: {message}");
    }

    protected void LogDebug(string message, params object[] values)
    {
        AWC.Log.Debug($"{Name}: {message}", values);
    }

    protected void LogInfo(string message)
    {
        AWC.Log.Info($"{Name}: {message}");
    }

    protected void LogError(string message)
    {
        AWC.Log.Error($"{Name}: {message}");
    }
}
