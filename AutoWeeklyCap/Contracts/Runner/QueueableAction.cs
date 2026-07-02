using ECommons.Automation.NeoTaskManager;

namespace AutoWeeklyCap.Contracts.Runner;

public abstract class QueueableAction
{
    protected abstract string Name { get; }

    protected void Enqueue(Func<bool> action, string description)
    {
        AWC.TaskManager.Enqueue(action, $"{Name}: {description}");
    }

    protected void Enqueue(Func<bool> action, string description, int timelimitMs)
    {
        AWC.TaskManager.Enqueue(
            action,
            $"{Name}: {description}",
            new TaskManagerConfiguration(timelimitMs)
        );
    }

    protected void EnqueueDelay(int ms)
    {
        AWC.TaskManager.EnqueueDelay(ms);
    }
}
