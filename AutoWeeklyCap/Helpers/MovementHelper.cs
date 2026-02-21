using ECommons.Automation.NeoTaskManager;
using ECommons.Automation.NeoTaskManager.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Helpers;

public static class MovementHelper
{
    public static bool MoveTo(Vector3? position) => MoveTo(position, 1.25f, 300_000);
    public static bool MoveTo(Vector3? position, float breakpoint) => MoveTo(position, breakpoint, 300_000);

    public static bool MoveTo(Vector3? position, float breakpoint, int timeLimitMs)
    {
        if (position == null)
            return false;

        if (!VNavMeshIPC.IsEnabled)
            return false;

        if (!PlayerHelper.IsReady)
            return false;

        AWC.TaskManager.InsertMulti(
            new TaskManagerTask(
                () => MoveToPosition((Vector3)position),
                "MovementHelper: start moving to location",
                new TaskManagerConfiguration(timeLimitMS: timeLimitMs)
            ),
            new TaskManagerTask(
                () => WaitForPosition((Vector3)position, breakpoint),
                "MovementHelper: waiting for player movement to location",
                new TaskManagerConfiguration(timeLimitMS: timeLimitMs)
            ),
            new DelayTask(250)
        );

        return true;
    }

    private static bool MoveToPosition(Vector3 position)
    {
        if (VNavMeshIPC.IsRunning() || !VNavMeshIPC.IsReady())
            return false;

        ChatHelper.RunCommand("automove off");
        VNavMeshIPC.SetTolerance(.25f);
        VNavMeshIPC.SetAlignCamera(true);
        VNavMeshIPC.PathfindAndMoveTo(position, false);

        return true;
    }

    private static unsafe bool WaitForPosition(Vector3 position, float breakpoint)
    {
        var distance = Vector3.Distance(position, Player.Position);

        if (PlayerHelper.IsMoving && !Player.Character->InCombat && distance >= 10)
        {
            if (CanSprint())
                ActionManager.Instance()->UseAction(ActionType.GeneralAction, 4);
        }

        if (distance > breakpoint)
            return false;

        VNavMeshIPC.Stop();

        return true;
    }

    private static unsafe bool CanSprint()
    {
        return ActionManager.Instance()->GetActionStatus(ActionType.GeneralAction, 4) == 0 &&
               ActionManager.Instance()->QueuedActionId != 4 &&
               !PlayerHelper.IsCasting;
    }
}
