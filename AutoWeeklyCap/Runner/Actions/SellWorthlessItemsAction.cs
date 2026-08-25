using System.Threading.Tasks;

using AutoWeeklyCap.Contracts.Runner;
using AutoWeeklyCap.Helpers.MarketBoard;

using ECommons.Automation.NeoTaskManager;

namespace AutoWeeklyCap.Runner.Actions;

public class SellWorthlessItemsAction : BaseAction
{
    protected override string Name => nameof(SellWorthlessItemsAction);
    protected override string[] AddonsToClose => ["SelectIconString", "SelectString", "Shop", "Talk", "SelectYesno"];

    protected override bool Run(params object[] args)
    {
        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled) {
            return false;
        }

        List<MarketBoardItem> marketBoardItems;

        EnqueueAsync(async () =>
        {
            // marketBoardItems =
            marketBoardItems = [new MarketBoardItem() { IsLoaded = true, ItemId = 13714, NqPrice = 500 }];
            if (marketBoardItems.Count == 0) {
                return;
            }

            List<MarketBoardItem> itemsToSell = marketBoardItems.Where(item => item.Price < 1000).ToList();
            if (itemsToSell.Count == 0) {
                return;
            }

            EnqueueActionTasks(itemsToSell);
        }, "checking marketboard prices");

        return true;
    }

    private void EnqueueActionTasks(List<MarketBoardItem> itemsToSell)
    {
        AWC.TaskManager.InsertMulti(
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
                    foreach (MarketBoardItem item in itemsToSell) {
                        AWC.Log.Debug($"Selling item: {item.ItemId}");
                    }
                }, $"{Name}: sell items"),
            new TaskManagerTask(() => AddonHelper.CloseAddons(AddonsToClose), $"{Name}: closing addons")
        );
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
