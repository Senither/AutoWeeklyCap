using AutoWeeklyCap.Contracts.Runner;

using ECommons.Configuration;
using ECommons.UIHelpers.AddonMasterImplementations;

// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.Runner.Actions;

public class AutoSpendTomestoneAction : BaseAction
{
    protected override string Name => nameof(AutoSpendTomestoneAction);
    protected override string[] AddonsToClose { get; } = ["ShopExchangeCurrency", "SelectIconString", "SelectYesno", "SelectString"];

    private const int LongTaskTimeout = 120_000;

    private const string MetricsKey = "TomestonesSpent";

    // Path 7.4 - Materials - Zircon @ Solution Nine (Nexus Arcade)
    private static readonly Vector3 MaterialVendorPosition = new(-185.5f, 0.6600001f, -28.45f);
    private const uint MaterialVendorDataID = 1049079u;
    private const uint MaterialVendorTerritoryID = 1186u;
    private const int MaterialVendorSelectionIndex = 3;
    private const string MaterialVendorAetheriteName = "Nexus Arcade";

    // Patch 7.4 - Relic - Ermina @ Phantom Village
    private static readonly Vector3 RelicVendorPosition = new(40.244816f, -1.1920929E-07f, 19.306528f);
    private const uint RelicVendorDataID = 1053904u;
    private const uint RelicVendorTerritoryID = 1278u;
    private const string RelicVendorAetheriteName = "Phantom Village";

    protected override bool Run(params object[] args)
    {
        var name = PlayerHelper.GetFullCharacterName();
        if (name is null) {
            return false;
        }

        var itemToBuy = TomestoneItemHelper.GetTomestoneItemFromNames(
            AWC.Config.GetOrRegisterCharacterOptions(name)?.PreferredTomestoneItemName,
            // AWC.Config.SpendUncappedTomestoneItemName
            ""
        );

        if (itemToBuy == null) {
            return false;
        }

        var quantity = itemToBuy.CalculateQuantityForGivenTomestones(CurrencyHelper.GetUncappedAcquiredTomestoneCount());
        if (quantity == 0) {
            return false;
        }

        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled) {
            return false;
        }

        LocationManager.Reset();

        if (InventoryHelper.GetEmptySlotsInBag() < 1) {
            return false;
        }

        var (position, territoryID, aetheriteName) = GetVendorLocation(itemToBuy.NPC);
        var (vendorId, sectionId) = GetVendorInteractData(itemToBuy.NPC);

        LogDebug($"Queueing buy attempt tasks for: [position: {position}, territory: {territoryID}, aetherite: {aetheriteName}, vendorId: {vendorId}, sectionId: {sectionId}]");

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.VentureDeliveryMoogle, "Spending tomestones");

        Enqueue(() =>
        {
            AWC.Runner.State.SetMetric(MetricsKey, (uint)CurrencyHelper.GetUncappedAcquiredTomestoneCount());
            return true;
        }, "store tomestone metrics");

        Enqueue(
            () => MovementHelper.TeleportTo(aetheriteName, territoryID),
            "start moving to territory"
        );

        Enqueue(
            () => MovementHelper.MoveTo(position),
            "start moving to npc location",
            LongTaskTimeout
        );

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("OpeningTomestoneVendorWindow", 250)) {
                return false;
            }

            var vendor = ObjectHelper.FindGameObject(vendorId, position);
            if (vendor == null) {
                return false;
            }

            unsafe {
                if (AddonHelper.TryGetReadyAddon("SelectIconString", out _)) {
                    AddonHelper.ClickSelectIconString(sectionId);
                } else if (AddonHelper.TryGetReadyAddon("ShopExchangeCurrency", out _)) {
                    return true;
                } else if (!AddonHelper.TryGetReadyAddon("SelectIconString", out _)) {
                    ObjectHelper.InteractWithObject(vendor);
                }
            }

            return false;
        }, "open window");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("BuyingTomestoneItem", 500)) {
                return false;
            }

            if (InventoryHelper.GetEmptySlotsInBag() < 1) {
                LogInfo($"Stopping {Name}, reason: no items slot left");
                return true;
            }

            unsafe {
                if (AddonHelper.TryGetReadyAddon("SelectYesno", out _)) {
                    AddonHelper.ClickSelectYesno();
                    return true;
                }

                if (!AddonHelper.TryGetReadyAddon("ShopExchangeCurrency", out var shopExchangeAddon)) {
                    return false;
                }

                var shopExchangeCurrency = new AddonMaster.ShopExchangeCurrency(shopExchangeAddon);
                var index = -1;

                foreach (var item in ShopHelper.GetAllShopExchangeCurrencyItems(shopExchangeCurrency)) {
                    if (itemToBuy.ItemId == item.ItemId) {
                        index = (int)item.Index;
                    }
                }

                if (index == -1) {
                    AWC.Log.Error($"Failed to find item with an ID of {itemToBuy.ItemId} in the current shop window.");
                    AWC.Log.Error("Please report the item Id you're trying to buy, and the shop you're buying from to the developers so they can fix it, thanks :)");
                    return true;
                }

                AddonHelper.ClickShopExchangeItem(index, quantity);
            }

            return false;
        }, "buy tomestone item");

        EnqueueDelay(500);
        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("BuyingTomestoneItemClose", 500)) {
                return false;
            }

            try {
                unsafe {
                    if (AddonHelper.TryGetReadyAddon("SelectYesno", out _)) {
                        return false;
                    }

                    if (!AddonHelper.TryGetReadyAddon("ShopExchangeCurrency", out var addonShopExchangeCurrency)) {
                        return true;
                    }

                    addonShopExchangeCurrency->Close(true);
                }
            } catch (Exception) {
                // ignored
            }

            return false;
        }, "close window");

        Enqueue(() =>
        {
            uint tomestones = 0;

            if (AWC.Runner.State.HasMetric(MetricsKey)) {
                uint before = AWC.Runner.State.PullMetric(MetricsKey);

                tomestones = (uint)(before - CurrencyHelper.GetUncappedAcquiredTomestoneCount());
            }

            AWC.Config.GetCurrentCharacterMetrics()?.IncrementWeeklyTomestoneSpentCounter(tomestones);
            EzConfig.Save();

            return true;
        }, "update metrics");

        return true;
    }

    protected static (Vector3, uint, string) GetVendorLocation(TomestoneNPC npc)
    {
        return npc switch
        {
            TomestoneNPC.Material => (MaterialVendorPosition, MaterialVendorTerritoryID, MaterialVendorAetheriteName),
            TomestoneNPC.Relic => (RelicVendorPosition, RelicVendorTerritoryID, RelicVendorAetheriteName),
            _ => throw new ArgumentOutOfRangeException(nameof(npc), npc, null)
        };
    }

    protected static (uint, int) GetVendorInteractData(TomestoneNPC npc)
    {
        return npc switch
        {
            TomestoneNPC.Material => (MaterialVendorDataID, MaterialVendorSelectionIndex),
            TomestoneNPC.Relic => (RelicVendorDataID, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(npc), npc, null)
        };
    }
}
