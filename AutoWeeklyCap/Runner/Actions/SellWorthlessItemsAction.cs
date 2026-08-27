using System.Threading.Tasks;

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

    protected override bool Run(params object[] args)
    {
        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled) {
            return false;
        }

        EnqueueAsync(async () =>
        {
            HashSet<uint> uniqueItemIds = GetUniqueItemIds();
            if (uniqueItemIds.Count == 0) {
                return;
            }

            List<MarketBoardItem> marketBoardItems = await MarketBoardHelper.GetMarketBoardPrices(Player.CurrentWorld.RowId, uniqueItemIds);
            if (marketBoardItems.Count == 0) {
                return;
            }

            List<MarketBoardItem> itemsToSell = marketBoardItems
                .Where(item => item is { IsLoaded: true, Price: < 1000 })
                .OrderBy(item => item.Price)
                .ToList();

            if (itemsToSell.Count == 0) {
                return;
            }

            // Debug: Remove later
            LogDebug($"Found {itemsToSell.Count} items that matches the filters, queueing up sell tasks");
            foreach (var itemToSell in itemsToSell) {
                if (InventoryHelper.TryGetSheetItemFromItemId(itemToSell.ItemId, out var item)) {
                    LogDebug($"Preparing to sell item: {item.Name} | ItemId={itemToSell.ItemId}, Price={itemToSell.Price}, ItemUICategory={item.ItemUICategory.RowId}, ItemSearchCategory={item.ItemSearchCategory.RowId}");
                }
            }

            EnqueueActionTasks(itemsToSell);
        }, "checking marketboard prices");

        return true;
    }

    private void EnqueueActionTasks(List<MarketBoardItem> itemsToSell)
    {
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
            new TaskManagerTask(() => AddonHelper.CloseAddons(AddonsToClose), $"{Name}: closing addons")
        ]);
    }

    private HashSet<uint> GetUniqueItemIds()
    {
        HashSet<uint> uniqueItemIds = [];

        foreach (var inventoryItem in InventoryHelper.GetInventoryItems()) {
            if (!InventoryHelper.TryGetSheetItemFromItemId(inventoryItem.ItemId, out var item)) {
                continue;
            }

            // Skip items with no sell price
            if (item.PriceLow == 0) {
                continue;
            }

            // Skip items that are not sellable on the marketboard
            if (item.ItemSearchCategory.RowId == 0) {
                continue;
            }

            // Materia
            if (item.ItemUICategory.RowId is 57) {
                continue;
            }

            // Glamour Prism & Dispeller & Dark Matter
            if (item.ItemUICategory.RowId is 60 or 48) {
                continue;
            }

            // Potions & Food
            if (item.ItemUICategory.RowId is 60 or 46 or 44) {
                continue;
            }

            // Gysahl Greens
            if (item.RowId is 4868) {
                continue;
            }

            // Minions
            if (item.ItemUICategory.RowId == 81) {
                continue;
            }

            // Triple Triad Cards
            if (item.ItemUICategory.RowId == 86) {
                continue;
            }

            LogDebug($"Adding item: {item.Name} | ItemId={inventoryItem.ItemId}, ItemUICategory={item.ItemUICategory.RowId}, ItemSearchCategory={item.ItemSearchCategory.RowId}");

            uniqueItemIds.AddIfNotExist(inventoryItem.ItemId);
        }

        return uniqueItemIds;
    }
}
