using ECommons.Automation.NeoTaskManager;
using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.Configuration;

using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Helpers;

public static class MovementHelper
{
    private static DateTime? _startedAt = null;
    private static DateTime? _movementCheckStartedAt = null;
    private static Vector3? _movementCheckStartPosition = null;
    private static int _movementCheckStuckCounter = 0;

    private static readonly TimeSpan StuckCheckWindow = TimeSpan.FromSeconds(3);
    private const float MinimumMovementDistance = 0.45f;

    private const string MetricsKey = "TeleportationFees";

    public static bool MoveTo(Vector3? position)
    {
        return MoveTo(position, 1.25f, 300_000);
    }

    public static bool MoveTo(Vector3? position, float breakpoint)
    {
        return MoveTo(position, breakpoint, 300_000);
    }

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

        _movementCheckStuckCounter = 0;

        CreateMovementToPositionTasks(position.Value, breakpoint, timeLimitMs);

        return true;
    }

    private static void CreateMovementToPositionTasks(Vector3 position, float breakpoint, int timeLimitMs)
    {
        if (_movementCheckStuckCounter > 3) {
            AWC.Log.Warning("MovementHelper: Player stuck detection has reached the limit, stopping runner");
            AWC.Runner.Abort();
            return;
        }

        AWC.TaskManager.InsertMulti(
            new TaskManagerTask(
                () => MoveToPosition(position),
                "MovementHelper: start moving to location",
                new TaskManagerConfiguration(timeLimitMs)
            ),
            new TaskManagerTask(
                () => CheckForMovement(position),
                "MovementHelper: check for movement",
                new TaskManagerConfiguration(timeLimitMs)
            ),
            new TaskManagerTask(
                () => WaitForPosition(position, breakpoint, timeLimitMs),
                "MovementHelper: waiting for player movement to location",
                new TaskManagerConfiguration(timeLimitMs)
            ),
            new DelayTask(250)
        );
    }

    public static bool TeleportTo(string aetheriteName, uint territoryId, int timeLimitMs = 90_000)
    {
        if (aetheriteName.Trim().Length == 0 || territoryId == 0) {
            return false;
        }

        if (!LifestreamIPC.IsEnabled) {
            return false;
        }

        if (!PlayerHelper.IsReady) {
            return false;
        }

        AWC.TaskManager.InsertMulti(
            new TaskManagerTask(
                () => AWC.Runner.State.SetMetric(MetricsKey, (uint)CurrencyHelper.GetGil()),
                "MovementHelper: prepare teleportation metrics",
                new TaskManagerConfiguration(timeLimitMs)
            ),
            new TaskManagerTask(
                () => TeleportToLocation(aetheriteName, territoryId),
                "MovementHelper: start teleporting to location",
                new TaskManagerConfiguration(timeLimitMs)
            ),
            new TaskManagerTask(
                () => CheckForTeleportationArrival(territoryId),
                "MovementHelper: wait for teleporting arrival",
                new TaskManagerConfiguration(timeLimitMs)
            ),
            new TaskManagerTask(
                UpdateTeleportationMetrics,
                "MovementHelper: update teleportation metrics",
                new TaskManagerConfiguration(timeLimitMs)
            ),
            new DelayTask(250)
        );

        return true;
    }

    public static bool TeleportTo(string aetheriteName, Func<bool> teleportationCondition, int timeLimitMs = 90_000)
    {
        if (aetheriteName.Trim().Length == 0) {
            return false;
        }

        if (!LifestreamIPC.IsEnabled) {
            return false;
        }

        if (!PlayerHelper.IsReady) {
            return false;
        }

        AWC.TaskManager.InsertMulti(
            new TaskManagerTask(
                () => AWC.Runner.State.SetMetric(MetricsKey, (uint)CurrencyHelper.GetGil()),
                "MovementHelper: prepare teleportation metrics",
                new TaskManagerConfiguration(timeLimitMs)
            ),
            new TaskManagerTask(
                () => TeleportToLocation(aetheriteName),
                "MovementHelper: start teleporting to location",
                new TaskManagerConfiguration(timeLimitMs)
            ),
            new TaskManagerTask(
                teleportationCondition.Invoke,
                "MovementHelper: wait for teleporting arrival",
                new TaskManagerConfiguration(timeLimitMs)
            ),
            new TaskManagerTask(
                UpdateTeleportationMetrics,
                "MovementHelper: update teleportation metrics",
                new TaskManagerConfiguration(timeLimitMs)
            ),
            new DelayTask(250)
        );

        return true;
    }

    private static bool TeleportToLocation(string aetheriteName)
    {
        if (!EzThrottler.Throttle("NavigatingToTerritory", 500)) {
            return false;
        }

        if (LifestreamIPC.IsBusy()) {
            return false;
        }

        LifestreamIPC.ExecuteCommand(aetheriteName);

        return true;
    }

    private static bool TeleportToLocation(string aetheriteName, uint territoryId)
    {
        if (!EzThrottler.Throttle("NavigatingToTerritory", 500)) {
            return false;
        }

        if (Player.Territory.RowId == territoryId) {
            return true;
        }

        if (LifestreamIPC.IsBusy()) {
            return false;
        }

        LifestreamIPC.ExecuteCommand(aetheriteName);

        return true;
    }

    private static bool CheckForTeleportationArrival(uint territoryId)
    {
        if (!EzThrottler.Throttle("NavigatingToTomestoneTerritory", 500)) {
            return false;
        }

        return Player.Territory.RowId == territoryId && PlayerHelper.IsReady && !LifestreamIPC.IsBusy();
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

        _startedAt = DateTime.UtcNow;
        _movementCheckStartedAt = DateTime.UtcNow;
        _movementCheckStartPosition = Player.Position;

        return true;
    }

    private static bool CheckForMovement(Vector3 position)
    {
        if (VNavMeshIPC.IsRunning() && PlayerHelper.IsMoving) {
            return true;
        }

        if (VNavMeshIPC.PathfindInProgress()) {
            return false;
        }

        if (_startedAt == null || DateTime.UtcNow - _startedAt.Value <= TimeSpan.FromSeconds(5)) {
            return false;
        }

        VNavMeshIPC.Rebuild();

        return MoveToPosition(position);
    }

    private static unsafe bool WaitForPosition(Vector3 position, float breakpoint, int timeLimitMs)
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
            if (_movementCheckStartedAt == null || _movementCheckStartPosition == null) {
                _movementCheckStartedAt = DateTime.UtcNow;
                _movementCheckStartPosition = Player.Position;
            } else if (DateTime.UtcNow - _movementCheckStartedAt.Value >= StuckCheckWindow) {
                var movedDistance = Vector3.Distance(Player.Position, _movementCheckStartPosition.Value);

                _movementCheckStartedAt = DateTime.UtcNow;
                _movementCheckStartPosition = Player.Position;

                if (movedDistance >= MinimumMovementDistance) {
                    return false;
                }

                AWC.Log.Warning("MovementHelper: player stuck detected, rebuilding vnavmesh and retrying");

                VNavMeshIPC.Stop();
                VNavMeshIPC.Rebuild();

                _movementCheckStuckCounter++;

                CreateMovementToPositionTasks(position, breakpoint, timeLimitMs);

                return true;
            }

            return false;
        }

        VNavMeshIPC.Stop();
        _startedAt = null;
        _movementCheckStartedAt = null;
        _movementCheckStartPosition = null;

        return true;
    }

    private static void UpdateTeleportationMetrics()
    {
        if (!AWC.Runner.State.HasMetric(MetricsKey)) {
            return;
        }

        uint before = AWC.Runner.State.PullMetric(MetricsKey);

        AWC.Config.GetCurrentCharacterMetrics()
            ?.IncrementGilSpentOnTeleportationFeesCounter((uint)(before - CurrencyHelper.GetGil()));

        EzConfig.Save();
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
