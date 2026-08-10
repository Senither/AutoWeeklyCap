using AutoWeeklyCap.Contracts.Runner;

using FFXIVClientStructs.FFXIV.Client.Game;

// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.Runner.Actions;

public class BuyFoodAction : BaseAction
{
    protected override string Name => nameof(BuyFoodAction);
    protected override string[] AddonsToClose => ["SelectIconString", "SelectString", "Shop", "SelectYesno"];

    private const int LongTaskTimeout = 120_000;

    private static readonly Vector3 VendorPosition = new(-53.860195f, 1.6000004f, 16.426605f);
    private const uint VendorDataID = 1011040u;
    private const uint VendorTerritoryID = 144u;
    private const string AetheriteName = "The Gold Saucer";

    protected override bool Run(params object[] args)
    {
        if (!QuestManager.IsQuestComplete(65970)) {
            LogInfo("Stopping food buying, reason: player has not completed quest 65970 (It Could Happen to You)");
            return false;
        }

        if (CurrencyHelper.GetGil() < 5_000) {
            LogInfo("Stopping food buying, reason: player has less than 5,000 gil left");
            return false;
        }

        if (InventoryHelper.GetEmptySlotsInBag() < 1) {
            return false;
        }

        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled) {
            return false;
        }

        LocationManager.Reset();
        using var title = TitleManager.RegisterTitle(BitmapFontIcon.OrangeDiamond, "Buying Food");

        Enqueue(
            () => MovementHelper.TeleportTo(AetheriteName, VendorTerritoryID),
            "start moving to territory"
        );

        Enqueue(
            () => MovementHelper.MoveTo(VendorPosition),
            "start moving to npc location",
            LongTaskTimeout
        );

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("OpeningVendorWindow", 250)) {
                return false;
            }

            var vendor = ObjectHelper.FindGameObject(VendorDataID, VendorPosition);
            if (vendor == null) {
                return false;
            }

            unsafe {
                if (AddonHelper.TryGetReadyAddon("Shop", out _)) {
                    return true;
                }

                if (AddonHelper.TryGetReadyAddon("SelectIconString", out _)) {
                    AddonHelper.ClickSelectIconString(0);
                } else {
                    ObjectHelper.InteractWithObject(vendor);
                }
            }

            return false;
        }, "open window");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("BuyingItem", 500)) {
                return false;
            }

            if (InventoryHelper.GetEmptySlotsInBag() < 1) {
                LogInfo($"{Name}: Stopping {Name}, reason: no items slot left");
                return true;
            }

            unsafe {
                if (AddonHelper.TryGetReadyAddon("SelectYesno", out _)) {
                    AddonHelper.ClickSelectYesno();
                    return true;
                }

                if (!AddonHelper.TryGetReadyAddon("Shop", out _)) {
                    return false;
                }

                AddonHelper.ClickShopItem(12, 99);

                return false;
            }
        }, "buy food");

        EnqueueDelay(500);

        Enqueue(() => AddonHelper.CloseAddons(AddonsToClose), "close shop window");

        return true;
    }
}
