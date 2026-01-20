using System;
using Dalamud.Game.ClientState.Conditions;
using ECommons;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoWeeklyCap.Runner.Actions;

public abstract class BaseAction
{
    protected abstract string Name { get; }
    protected virtual string[] AddonsToClose { get; } = [];

    public bool Invoke()
    {
        if (!IsPlayerAvailable())
            return false;

        if (!Run())
            return false;

        if (AddonsToClose.Length > 0)
            AutoWeeklyCap.TaskManager.Insert(CloseAddons, $"{Name}: closing addons");

        return true;
    }

    protected abstract bool Run();

    protected static bool IsPlayerAvailable()
    {
        if (Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51])
            return false;

        return AutoWeeklyCap.PlayerState.IsLoaded && Player.Available;
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
        AutoWeeklyCap.TaskManager.Enqueue(action, $"{Name}: {description}");
    }

    protected void Enqueue(Func<bool> action, string description, int timelimitMS)
    {
        AutoWeeklyCap.TaskManager.Enqueue(
            action,
            $"{Name}: {description}",
            new TaskManagerConfiguration(timeLimitMS: timelimitMS)
        );
    }

    protected void EnqueueDelay(int ms)
    {
        AutoWeeklyCap.TaskManager.EnqueueDelay(ms);
    }

    protected void LogDebug(string message)
    {
        AutoWeeklyCap.Log.Debug($"{Name}: {message}");
    }

    protected void LogInfo(string message)
    {
        AutoWeeklyCap.Log.Info($"{Name}: {message}");
    }
}
