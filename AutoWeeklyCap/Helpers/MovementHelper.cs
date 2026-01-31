using ECommons.Automation.NeoTaskManager;
using ECommons.Automation.NeoTaskManager.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Helpers;

public static class MovementHelper
{
    public static bool MoveTo(Vector3 position) => MoveTo(position, 300_000);

    public static bool MoveTo(Vector3 position, int timeLimitMs)
    {
        if (!VNavMeshIPC.IsEnabled)
            return false;

        if (!PlayerHelper.IsReady)
            return false;

        AWC.TaskManager.InsertMulti(
            new TaskManagerTask(
                () => MoveToPosition(position),
                "MovementHelper: start moving to location",
                new TaskManagerConfiguration(timeLimitMS: timeLimitMs)
            ),
            new TaskManagerTask(
                () => WaitForPosition(position),
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

    private static unsafe bool WaitForPosition(Vector3 position)
    {
        var distance = Vector3.Distance(position, Player.Position);

        if (PlayerHelper.IsMoving && !Player.Character->InCombat && distance >= 10)
        {
            if (CanSprint())
                ActionManager.Instance()->UseAction(ActionType.GeneralAction, 4);
        }

        if (distance > 1.25)
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
