using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoWeeklyCap.Runner.Actions;

public abstract class BaseAction
{
    public bool Invoke()
    {
        if (!IsPlayerAvailable())
            return false;

        return Run();
    }

    protected abstract bool Run();

    protected static bool IsPlayerAvailable()
    {
        if (Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51])
            return false;

        return AutoWeeklyCap.PlayerState.IsLoaded && Player.Available;
    }
}
