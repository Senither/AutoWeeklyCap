using AutoWeeklyCap.Contracts.Runner;

using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Runner.Actions.Safezones;

public class EnterFcHouseAction : BaseAction
{
    protected override string Name => nameof(EnterFcHouseAction);
    protected override string[] AddonsToClose { get; } = ["SelectYesno", "SelectString", "HousingMenu"];

    private const int LongTaskTimeout = 120_000;
    private const float MaxDistance = 50f;
    private const string LifestreamFcCommand = "FC";

    protected override bool Run(params object[] args)
    {
        if (!LifestreamIPC.IsEnabled || !VNavMeshIPC.IsEnabled) {
            return false;
        }

        if (HousingHelper.IsInsideFC()) {
            return false;
        }

        if (!LifestreamIPC.HasFreeCompanyHouse()) {
            LogDebug("Player has no FC house");
            return false;
        }

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.WatchingCutscene, "Entering FC house");

        if (LocationManager.GetLastKnownLocation() != Safezone.FreeCompany) {
            TeleportToFreeCompany();
        }

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("MoveToFCHouseEntrance", 250)) {
                return false;
            }

            if (HousingHelper.IsInsideFC()) {
                return true;
            }

            var gameObject = ObjectHelper.FindEnteranceGameObject(MaxDistance);
            if (gameObject == null) {
                LogDebug("Unable to find a valid FC house entrance.");
                return false;
            }

            return MovementHelper.MoveTo(gameObject.Position, 4.5f);
        }, "move to FC house entrance");

        Enqueue(() => PlayerHelper.IsReady, "waiting for player");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("EnteringFCHouse", 250)) {
                return false;
            }

            if (HousingHelper.IsInsideFC()) {
                return true;
            }

            var gameObject = ObjectHelper.FindEnteranceGameObject(MaxDistance);
            if (gameObject == null) {
                return false;
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
        }, "enter FC house");

        Enqueue(() => PlayerHelper.IsReady && HousingHelper.IsInsideFC(), "wait for player to enter FC house");
        EnqueueDelay(1500);

        return true;
    }

    private void TeleportToFreeCompany()
    {
        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("NavigatingToFCHousePlot", 500)) {
                return false;
            }

            if (LifestreamIPC.IsBusy()) {
                return false;
            }

            LifestreamIPC.ExecuteCommand(LifestreamFcCommand);
            return true;
        }, "teleport to FC plot");

        Enqueue(
            () => IsInFcHouseTerritory() && PlayerHelper.IsReady && !LifestreamIPC.IsBusy(),
            "wait for FC plot teleport",
            LongTaskTimeout
        );
    }

    private static bool IsInFcHouseTerritory()
    {
        var houseId = HousingManager.GetOwnedHouseId(EstateType.FreeCompanyEstate);

        return (houseId.Id != 0 && Player.Territory.RowId == houseId.TerritoryTypeId) || HousingHelper.IsInsideFC();
    }
}
