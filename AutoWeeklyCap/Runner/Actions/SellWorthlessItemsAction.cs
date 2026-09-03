using System.Threading.Tasks;

using AutoWeeklyCap.Config;
using AutoWeeklyCap.Contracts.Runner;
using AutoWeeklyCap.Helpers.MarketBoard;

using ECommons.Automation.NeoTaskManager;

using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace AutoWeeklyCap.Runner.Actions;

public class SellWorthlessItemsAction : BaseAction
{
    protected override string Name => nameof(SellWorthlessItemsAction);
    protected override string[] AddonsToClose => ["SelectIconString", "SelectString", "Shop", "ContextMenu", "Talk", "SelectYesno"];

    private const string MetricsKey = "GilEarnedFromVendoredItems";

    protected override bool Run(params object[] args)
    {
        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled) {
            return false;
        }

        EnqueueAsync(async () =>
        {
            var itemsToSell = await MarketBoardHelper.GetFilteredMarketBoardItemsFromInventory();
            if (itemsToSell.Count == 0) {
                return;
            }

            // Debug: Remove later
            LogDebug($"Found {itemsToSell.Count} items that matches the filters, queueing up sell tasks");
            foreach (var itemToSell in itemsToSell) {
                if (InventoryHelper.TryGetSheetItemFromItemId(itemToSell.ItemId, out var item)) {
                    LogDebug($"Preparing to sell item: {item.Name} | ItemId={itemToSell.ItemId}, Price={itemToSell.GetPrice(item.CanBeHq)}, ItemUICategory={item.ItemUICategory.RowId}, ItemSearchCategory={item.ItemSearchCategory.RowId}");
                }
            }

            EnqueueActionTasks(itemsToSell);
        }, "checking marketboard prices");

        return true;
    }

    private void EnqueueActionTasks(List<MarketBoardItem> itemsToSell)
    {
        LocationManager.Reset();

        List<MarketBoardItem> remainingItemsToSell = itemsToSell.ToList();

        AWC.TaskManager.InsertMulti([
            .. CaptureQueuedActions(() => ActionInstance.LeaveGrandCompanyInn.Invoke()),
            new TaskManagerTask(
                () => MovementHelper.TeleportTo(GrandCompanyHelper.AetheriteName, GrandCompanyHelper.TerritoryId),
                $"{Name}: move to territory"
            ),
            new TaskManagerTask(
                () => MovementHelper.MoveTo(GrandCompanyHelper.SellVendorLocation),
                $"{Name}: move to location"
            ),
            new TaskManagerTask(() =>
                {
                    if (!EzThrottler.Throttle("OpeningVendorWindow", 250)) {
                        return false;
                    }

                    var vendor = ObjectHelper.FindGameObject(GrandCompanyHelper.SellVendorId, GrandCompanyHelper.SellVendorLocation);
                    if (vendor == null) {
                        return false;
                    }

                    unsafe {
                        if (AddonHelper.TryGetReadyAddon("Shop", out _)) {
                            return true;
                        }

                        if (AddonHelper.TryGetReadyAddon("Talk", out _)) {
                            AddonHelper.ClickTalk();
                        } else {
                            ObjectHelper.InteractWithObject(vendor);
                        }
                    }

                    return false;
                }, $"{Name}: open window"),
            new TaskManagerTask(
                () => AWC.Runner.State.SetMetric(MetricsKey, (uint)CurrencyHelper.GetGil()),
                $"{Name}: prepare gil earned metrics"
            ),
            new TaskManagerTask(() =>
                {
                    if (remainingItemsToSell.Count == 0) {
                        return true;
                    }

                    if (!EzThrottler.Throttle("SellingItems", 250)) {
                        return false;
                    }

                    unsafe {
                        if (AddonHelper.TryGetReadyAddon("SelectYesno", out var yesNoAddon)) {
                            AddonHelper.ClickSelectYesno();
                            return false;
                        }

                        if (AddonHelper.TryGetReadyAddon("ContextMenu", out var contextMenu)) {
                            AddonHelper.FireCallBack(contextMenu, true, 0, 0);
                            return false;
                        }

                        if (!AddonHelper.TryGetReadyAddon("Shop", out _)) {
                            return true;
                        }

                        MarketBoardItem nextItem = remainingItemsToSell[0];
                        InventoryItem inventoryItem = InventoryHelper.GetInventoryItems()
                            .FirstOrDefault(item => item.ItemId == nextItem.ItemId);

                        if (inventoryItem.ItemId == 0) {
                            remainingItemsToSell.RemoveAt(0);
                            return false;
                        }

                        AgentInventoryContext.Instance()->OpenForItemSlot(inventoryItem.Container, inventoryItem.Slot, 0, 0);
                        LogDebug($"Selling item: {inventoryItem.ItemId} ({inventoryItem.Quantity} qty) | Slot={inventoryItem.Slot}, Container={inventoryItem.Container}");

                        return false;
                    }
                }, $"{Name}: sell items"),
            new TaskManagerTask(
                UpdateGilEarnedMetrics,
                "MovementHelper: update gil earned metrics"
            ),
            new TaskManagerTask(() => AddonHelper.CloseAddons(AddonsToClose), $"{Name}: closing addons")
        ]);
    }

    private static void UpdateGilEarnedMetrics()
    {
        if (!AWC.Runner.State.HasMetric(MetricsKey)) {
            return;
        }

        uint before = AWC.Runner.State.PullMetric(MetricsKey);

        AWC.Config.GetCurrentCharacterMetrics()
            ?.IncrementGilEarnedFromSellingItemsCounter((uint)(CurrencyHelper.GetGil() - before));

        Configuration.Save();
    }
}
