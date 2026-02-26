using Dalamud.Game.ClientState.Conditions;
using ECommons.Automation.NeoTaskManager;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoWeeklyCap.Runner.Actions;

public abstract class BaseAction
{
    protected abstract string Name { get; }
    protected virtual string[] AddonsToClose { get; } = [];

    public string GetName() => Name;

    /// <summary>
    /// Will first check if the player is both valid and not between loading zones,
    /// then attempt to run the action which will queue all the tasks within the
    /// task manager, and then finally close all the addons the action might
    /// interact with to ensure all the addons are in the correct state.
    /// </summary>
    /// <returns>True if the action was invoked successfully</returns>
    public bool Invoke(params object[] args)
    {
        if (!IsPlayerAvailable())
            return false;

        if (!Run(args))
            return false;

        if (AddonsToClose.Length > 0)
            AWC.TaskManager.Insert(CloseAddons, $"{Name}: closing addons");

        return true;
    }

    /// <summary>
    /// Will invoke the run method while skipping all the validation checks to
    /// ensure the player is logged in, valid, and addons that the action
    /// might interact with are closed and in the right state.
    /// </summary>
    /// <returns>True if the action was invoked successfully</returns>
    public bool ForceInvoke(params object[] args) => Run(args);

    protected abstract bool Run(params object[] args);

    protected static bool IsPlayerAvailable()
    {
        if (Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51])
            return false;

        if (PlayerHelper.InDuty)
            return false;

        return AWC.PlayerState.IsLoaded && Player.Available;
    }

    private bool CloseAddons()
    {
        foreach (var name in AddonsToClose)
        {
            try
            {
                unsafe
                {
                    if (GenericHelpers.TryGetAddonByName(name, out AtkUnitBase* atkUnitBase) && atkUnitBase->IsReady())
                    {
                        atkUnitBase->FireCallbackInt(-1);
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        return true;
    }

    // TaskManager proxy methods

    protected void Enqueue(Func<bool> action, string description)
    {
        AWC.TaskManager.Enqueue(action, $"{Name}: {description}");
    }

    protected void Enqueue(Func<bool> action, string description, int timelimitMS)
    {
        AWC.TaskManager.Enqueue(
            action,
            $"{Name}: {description}",
            new TaskManagerConfiguration(timeLimitMS: timelimitMS)
        );
    }

    protected void EnqueueDelay(int ms)
    {
        AWC.TaskManager.EnqueueDelay(ms);
    }

    protected void LogDebug(string message)
    {
        AWC.Log.Debug($"{Name}: {message}");
    }

    protected void LogInfo(string message)
    {
        AWC.Log.Info($"{Name}: {message}");
    }
}
