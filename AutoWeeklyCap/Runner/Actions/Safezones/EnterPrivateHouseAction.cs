using AutoWeeklyCap.Contracts.Runner;
using AutoWeeklyCap.IPC.Lifestream;

using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Runner.Actions.Safezones;

public class EnterPrivateHouseAction : BaseAction
{
    protected override string Name => nameof(EnterPrivateHouseAction);
    protected override string[] AddonsToClose { get; } = ["SelectYesno", "SelectString", "HousingMenu"];

    private const int LongTaskTimeout = 120_000;
    private const float MaxDistance = 20f;
    private const string LifestreamHomeCommand = "Home";

    protected override bool Run(params object[] args)
    {
        if (!LifestreamIPC.IsEnabled || !VNavMeshIPC.IsEnabled) {
            return false;
        }

        if (HousingHelper.IsInsideHouse()) {
            return false;
        }

        if (!LifestreamIPC.HasPrivateHouse()) {
            LogDebug("Player has no private house");
            return false;
        }

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.WatchingCutscene, "Entering private house");

        if (LocationManager.GetLastKnownLocation() != Safezone.PrivateHouse && !IsOnPrivateHousePlot()) {
            TeleportToPrivateHouse();
        }

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("MoveToPrivateHouseEntrance", 250)) {
                return false;
            }

            if (HousingHelper.IsInsideHouse()) {
                return true;
            }

            var gameObject = ObjectHelper.FindEnteranceGameObject(MaxDistance);
            if (gameObject == null) {
                LogDebug("Unable to find a valid private house entrance.");
                return false;
            }

            return MovementHelper.MoveTo(gameObject.Position, 4.5f);
        }, "move to house entrance");

        Enqueue(() => PlayerHelper.IsReady, "waiting for player");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("EnteringPrivateHouse", 250)) {
                return false;
            }

            if (HousingHelper.IsInsideHouse()) {
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
        }, "enter house");

        Enqueue(() => PlayerHelper.IsReady && HousingHelper.IsInsideHouse(), "wait for player to enter house");
        EnqueueDelay(1500);

        return true;
    }

    private static bool IsOnPrivateHousePlot()
    {
        (int Kind, int Ward, int Plot)? info = LifestreamIPC.GetCurrentPlotInfo();
        if (info == null) {
            return false;
        }

        (HousePathData? Private, HousePathData? FC) data = LifestreamIPC.GetHousePathData(Player.CID);
        if (data.Private == null) {
            return false;
        }

        return data.Private.Plot == info.Value.Plot
               && data.Private.Ward == info.Value.Ward
               && data.Private.ResidentialDistrict == info.Value.Kind;
    }

    private void TeleportToPrivateHouse()
    {
        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("NavigatingToPrivateHousePlot", 500)) {
                return false;
            }

            if (LifestreamIPC.IsBusy()) {
                return false;
            }

            LifestreamIPC.ExecuteCommand(LifestreamHomeCommand);
            return true;
        }, "teleport to house plot");

        Enqueue(
            () => IsInPrivateHouseTerritory() && PlayerHelper.IsReady && !LifestreamIPC.IsBusy(),
            "wait for house plot teleport",
            LongTaskTimeout
        );
    }

    private static bool IsInPrivateHouseTerritory()
    {
        if (HousingHelper.IsInsideHouse()) {
            return true;
        }

        foreach (var houseId in GetPrivateHouseIds()) {
            if (houseId.Id != 0 && Player.Territory.RowId == houseId.TerritoryTypeId) {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<HouseId> GetPrivateHouseIds()
    {
        yield return HousingManager.GetOwnedHouseId(EstateType.PersonalEstate);

        for (var i = 0; i < 2; i++) {
            yield return HousingManager.GetOwnedHouseId(EstateType.SharedEstate, i);
        }
    }
}
