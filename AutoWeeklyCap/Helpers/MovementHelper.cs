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
        if (position == null) {
            return false;
        }

        if (!VNavMeshIPC.IsEnabled) {
            return false;
        }

        if (!PlayerHelper.IsReady) {
            return false;
        }

        if (Vector3.Distance(Player.Position, position.Value) < breakpoint) {
            return true;
        }

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
        if (VNavMeshIPC.IsRunning() || !VNavMeshIPC.IsReady()) {
            return false;
        }

        ChatHelper.RunCommand("automove off");
        VNavMeshIPC.SetTolerance(.25f);
        VNavMeshIPC.SetAlignCamera(true);
        VNavMeshIPC.PathfindAndMoveTo(position, false);

        return true;
    }

    private static unsafe bool WaitForPosition(Vector3 position, float breakpoint)
    {
        var distance = Vector3.Distance(position, Player.Position);

        if (PlayerHelper.IsMoving && !Player.Character->InCombat && distance >= 10) {
            if (CanUseSprint) {
                ActionManager.Instance()->UseAction(ActionType.GeneralAction, 4);
            }

            if (CanUsePeloton) {
                ActionManager.Instance()->UseAction(ActionType.Action, 7557);
            }
        }

        if (distance > breakpoint) {
            return false;
        }

        VNavMeshIPC.Stop();

        return true;
    }

    private static unsafe bool CanUseSprint =>
        ActionManager.Instance()->GetActionStatus(ActionType.GeneralAction, 4) == 0 &&
        ActionManager.Instance()->QueuedActionId != 4 &&
        !PlayerHelper.IsCasting;

    private static unsafe bool CanUsePeloton =>
        ActionManager.Instance()->GetActionStatus(ActionType.Action, 7557) == 0 &&
        ActionManager.Instance()->QueuedActionId != 7557 &&
        Player.Status.All(x => x.StatusId != 1199);
}
