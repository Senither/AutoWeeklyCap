using AutoWeeklyCap.Contracts.Runner;

using ECommons.Configuration;

using FFXIVClientStructs.FFXIV.Component.GUI;

// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.Runner.Actions;

public class AutoSpendTomestoneAction : BaseAction
{
    protected override string Name => nameof(AutoSpendTomestoneAction);
    protected override string[] AddonsToClose { get; } = ["ShopExchangeCurrency", "SelectIconString", "SelectYesno", "SelectString"];

    private const int LongTaskTimeout = 120_000;
    private const int MaterialVendorFramerKitCount = 11;

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

    // Pointers for game instances to open and interact with windows
    private unsafe AtkUnitBase* AddonSelectIconString = null;
    private unsafe AtkUnitBase* AddonShopExchangeCurrency = null;

    private const string MetricsKey = "TomestonesSpent";

    protected override bool Run(params object[] args)
    {
        var name = PlayerHelper.GetFullCharacterName();
        if (name is null) {
            return false;
        }

        var characterOptions = AWC.Config.GetOrRegisterCharacterOptions(name);
        var itemToBuy = TomestoneItemHelper.GetTomestoneItemFromNames(
            characterOptions?.PreferredTomestoneItemName,
            AWC.Config.SpendUncappedTomestoneItemName
        );

        if (itemToBuy == null) {
            return false;
        }

        var shopItemIndex = itemToBuy.Index;
        if (itemToBuy.NPC == TomestoneNPC.Material && characterOptions?.MaterialVendorFramerKitsVisible == false) {
            shopItemIndex -= MaterialVendorFramerKitCount;
        }

        var quantity = itemToBuy.CalculateQuantityForGivenTomestones(CurrencyHelper.GetUncappedAcquiredTomestoneCount());
        if (quantity == 0) {
            return false;
        }

        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled) {
            return false;
        }

        LocationManager.Reset();

        unsafe {
            if (InventoryHelper.GetEmptySlotsInBag() < 1) {
                return false;
            }

            // Reset state before starting
            AddonSelectIconString = null;
            AddonShopExchangeCurrency = null;
        }

        var (position, territoryID, aetheriteName) = GetVendorLocation(itemToBuy.NPC);
        var (vendorId, sectionId) = GetVendorInteractData(itemToBuy.NPC);

        LogDebug($"Queueing buy attempt tasks for: [position: {position}, territory: {territoryID}, aetherite: {aetheriteName}, vendorId: {vendorId}, sectionId: {sectionId}, itemIndex: {shopItemIndex}]");

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
                if (GenericHelpers.TryGetAddonByName("SelectIconString", out AddonSelectIconString) && GenericHelpers.IsAddonReady(AddonSelectIconString)) {
                    AddonHelper.ClickSelectIconString(sectionId);
                } else if (GenericHelpers.TryGetAddonByName("ShopExchangeCurrency", out AddonShopExchangeCurrency) && GenericHelpers.IsAddonReady(AddonShopExchangeCurrency)) {
                    return true;
                } else if (!GenericHelpers.TryGetAddonByName("SelectIconString", out AddonSelectIconString)) {
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

            unsafe {
                if (GenericHelpers.TryGetAddonByName("SelectYesno", out AddonSelectIconString) && GenericHelpers.IsAddonReady(AddonSelectIconString)) {
                    AddonHelper.ClickSelectYesno();
                    return true;
                }

                if (GenericHelpers.TryGetAddonByName("ShopExchangeCurrency", out AddonShopExchangeCurrency) && GenericHelpers.IsAddonReady(AddonShopExchangeCurrency)) {
                    AddonHelper.ClickShopExchangeItem(shopItemIndex, quantity);
                }
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
