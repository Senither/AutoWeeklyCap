using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;

using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Runner.Actions.Safezones;

public class LeavePrivateHouseAction : BaseAction
{
    protected override string Name => nameof(LeavePrivateHouseAction);

    protected override string[] AddonsToClose { get; } = ["SelectYesno", "SelectString", "HousingMenu"];

    protected override bool Run(params object[] args)
    {
        if (!HousingHelper.IsInsideHouse()) {
            return false;
        }

        var gameObject = FindHouseExit();
        if (gameObject == null) {
            LogDebug("Unable to find a valid private house exit.");
            return false;
        }

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.WatchingCutscene, "Leaving private house");

        LogDebug($"Leaving via {gameObject.Name.TextValue} ({gameObject.EntityId})");

        Enqueue(() =>
        {
            if (EzThrottler.Throttle("MoveToPrivateHouseExit", 250)) {
                return false;
            }

            var distance = Vector3.Distance(Player.Position, gameObject.Position);
            if (distance <= 4.5f) {
                return true;
            }

            unsafe {
                var hm = HousingManager.Instance();

                return hm != null
                    ? hm->MoveToEntry()
                    : VNavMeshIPC.IsEnabled && MovementHelper.MoveTo(gameObject.Position, 4.75f);
            }
        }, "move to house exit");

        Enqueue(() => PlayerHelper.IsReady, "waiting for player");

        Enqueue(() =>
        {
            if (EzThrottler.Throttle("LeavingPrivateHouse", 250)) {
                return false;
            }

            if (!HousingHelper.IsInsideHouse()) {
                return true;
            }

            unsafe {
                if (AddonHelper.TryGetReadyAddon("SelectYesno", out _)) {
                    AddonHelper.ClickSelectYesno();
                } else if (AddonHelper.TryGetReadyAddon("SelectString", out _)) {
                    AddonHelper.ClickSelectString(0);
                } else if (PlayerHelper.IsReady) {
                    ObjectHelper.InteractWithObject(gameObject);
                }
            }

            return false;
        }, "leave house");

        Enqueue(() => PlayerHelper.IsReady && !HousingHelper.IsInsideHouse(), "wait for player to leave house");
        EnqueueDelay(1500);

        return true;
    }

    private static IGameObject? FindHouseExit()
    {
        IGameObject? gameObject = null;
        var closestDistance = float.MaxValue;

        foreach (var obj in Svc.Objects) {
            if (obj.ObjectKind != ObjectKind.EventObj || !obj.IsTargetable) {
                continue;
            }

            var distance = Vector3.Distance(obj.Position, Player.Position);
            if (distance <= 0.25f || !(distance < closestDistance)) {
                continue;
            }

            gameObject = obj;
            closestDistance = distance;
        }

        return gameObject;
    }
}
