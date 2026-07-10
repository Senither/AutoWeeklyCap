using AutoWeeklyCap.Contracts.Runner;

using Dalamud.Game.Inventory;

using FFXIVClientStructs.FFXIV.Client.Game;

using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Runner.Actions;

public class MoveInventoryItemsToSaddlebagAction : BaseAction
{
    protected override string Name => nameof(MoveInventoryItemsToSaddlebagAction);
    protected override string[] AddonsToClose => ["SelectIconString", "SelectString", "SelectYesno", "InventoryBuddy", "InventoryExpansion"];

    private const int LongTaskTimeout = 120_000;

    protected override bool Run(params object[] args)
    {
        if (!QuestManager.IsQuestComplete(66698)) {
            LogInfo("Stopping cleaning up inventory, reason: player has not completed quest 66698 (My Feisty Little Chocobo)");
            return false;
        }

        ExcelSheet<Item> itemSheet = Svc.Data.GetExcelSheet<Item>();

        Enqueue(() => !PlayerHelper.IsOccupied, "waiting for player not to be occupied");
        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("OpenSaddlebagInventory", 500)) {
                return false;
            }

            unsafe {
                if (AddonHelper.TryGetReadyAddon("InventoryBuddy", out _)) {
                    return true;
                }
            }

            ChatHelper.RunCommand("saddlebag");
            return false;
        }, "opening saddle bag");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("MoveInventoryItem", 1000)) {
                return false;
            }

            var saddlebagItems = InventoryHelper.GetSaddleBagItems()
                .Where(item => !item.IsEmpty)
                .ToList();

            if (saddlebagItems.Count == 0) {
                LogDebug("No saddlebag items found to stack.");
                return true;
            }

            var inventoryItems = InventoryHelper.GetPlayerInventoryItems()
                .Where(item => !item.IsEmpty)
                .ToList();

            foreach (var saddlebagItem in saddlebagItems) {
                var maxStackSize = GetItemMaxStack(itemSheet, saddlebagItem.BaseItemId);
                if (saddlebagItem.Quantity >= maxStackSize) {
                    continue;
                }

                foreach (var inventoryItem in inventoryItems) {
                    if (inventoryItem.BaseItemId != saddlebagItem.BaseItemId) {
                        continue;
                    }

                    if (inventoryItem.Quantity <= 0) {
                        continue;
                    }

                    if (inventoryItem.IsHq != saddlebagItem.IsHq || inventoryItem.IsCollectable != saddlebagItem.IsCollectable) {
                        continue;
                    }

                    if (!TryMoveStack(inventoryItem, saddlebagItem)) {
                        continue;
                    }

                    LogDebug($"Moving item {inventoryItem.BaseItemId} from {inventoryItem.ContainerType}/{inventoryItem.InventorySlot} to {saddlebagItem.ContainerType}/{saddlebagItem.InventorySlot}");
                    return false;
                }
            }

            LogDebug("Finished stacking matching items into saddlebag.");
            return true;
        }, "moving items to saddlebag", LongTaskTimeout);

        Enqueue(() => AddonHelper.CloseAddons(AddonsToClose), "closing addons");

        return true;
    }

    private static uint GetItemMaxStack(ExcelSheet<Item> itemSheet, uint itemId)
    {
        if (itemId == 0) {
            return 1;
        }

        try {
            var item = itemSheet.GetRow(itemId);
            return item.RowId > 0 ? item.StackSize : 1;
        } catch (Exception) {
            return 1;
        }
    }

    private static bool TryMoveStack(GameInventoryItem from, GameInventoryItem to)
    {
        unsafe {
            var inventoryManager = InventoryManager.Instance();
            if (inventoryManager == null) {
                return false;
            }

            var result = inventoryManager->MoveItemSlot(
                (InventoryType)from.ContainerType,
                (ushort)from.InventorySlot,
                (InventoryType)to.ContainerType,
                (ushort)to.InventorySlot,
                true
            );

            return result >= 0;
        }
    }
}
