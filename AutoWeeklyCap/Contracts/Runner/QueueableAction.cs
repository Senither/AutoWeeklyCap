using System.Threading.Tasks;

using AutoWeeklyCap.Exceptions;

using ECommons.Automation.NeoTaskManager;
using ECommons.Automation.NeoTaskManager.Tasks;

namespace AutoWeeklyCap.Contracts.Runner;

public abstract class QueueableAction
{
    private static List<TaskManagerTask> CapturedActions = [];
    private static bool IsCapturing = false;

    protected abstract string Name { get; }

    protected void Enqueue(Func<bool> action, string description)
    {
        EnqueueOrCapture(
            new TaskManagerTask(action, $"{Name}: {description}"),
            () => AWC.TaskManager.Enqueue(action, $"{Name}: {description}")
        );
    }

    protected void EnqueueAsync(Func<Task> action, string description)
    {
        EnqueueOrCapture(
            new TaskManagerTask(WrappedAction, $"{Name}: {description}"),
            () => AWC.TaskManager.Enqueue(WrappedAction, $"{Name}: {description}")
        );

        return;

        async void WrappedAction()
        {
            try {
                await action();
            } catch (Exception) {
                // ignored
            }
        }
    }

    protected void Enqueue(Func<bool> action, string description, int timelimitMs)
    {
        var configuration = new TaskManagerConfiguration(timelimitMs);

        EnqueueOrCapture(
            new TaskManagerTask(action, $"{Name}: {description}", configuration),
            () => AWC.TaskManager.Enqueue(action, $"{Name}: {description}", configuration)
        );
    }

    protected void EnqueueDelay(int ms)
    {
        EnqueueOrCapture(new DelayTask(ms), () => AWC.TaskManager.EnqueueDelay(ms));
    }

    protected static List<TaskManagerTask> CaptureQueuedActions(Action action)
    {
        if (IsCapturing) {
            throw InvalidCaptureStateException.CreateCaptureIsAlreadyEnabled();
        }

        IsCapturing = true;
        CapturedActions = [];

        try {
            action();
        } catch (Exception ex) {
            AWC.Log.Error("[QueueableAction] Failed to capture queued action, error:", ex);
        } finally {
            IsCapturing = false;
        }

        return CapturedActions;
    }

    private static void EnqueueOrCapture(TaskManagerTask task, Action enqueue)
    {
        if (IsCapturing) {
            CapturedActions.Add(task);
        } else {
            enqueue();
        }
    }
}
