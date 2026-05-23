using FFXIVClientStructs.FFXIV.Client.Game;

using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Helpers;

public static unsafe class InventoryHelper
{
    private static readonly uint[] CanHaveOffhand = [2, 6, 8, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32];
    private static readonly uint[] IgnoreCategory = [105];

    internal static bool CanRepair()
    {
        return CanRepair(AWC.Config.RepairPercentage);
    }

    internal static bool CanRepair(uint percent)
    {
        return (LowestEquippedItem().Condition / 300f) <= percent;
    }

    internal static InventoryItem LowestEquippedItem()
    {
        var equippedItems = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems);

        uint lowestCondition = 999999;
        uint lowestItem = 0;

        for (uint i = 0; i < 13; i++) {
            var item = equippedItems->Items[i];
            if (lowestCondition <= item.Condition) {
                continue;
            }

            lowestItem = i;
            lowestCondition = item.Condition;
        }

        return equippedItems->Items[lowestItem];
    }

    internal static int GetItemsNeedingRepairCount(uint percent)
    {
        var equippedItems = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems);
        var itemsNeedingRepair = 0;

        for (uint i = 0; i < 13; i++) {
            var item = equippedItems->Items[i];

            if (item.Condition / 300f <= percent) {
                itemsNeedingRepair++;
            }
        }

        return itemsNeedingRepair;
    }

    internal static int GetDarkMatterCount()
    {
        foreach (var dm in Svc.Data.Excel.GetSheet<ItemRepairResource>()) {
            var count = InventoryManager.Instance()->GetInventoryItemCount(dm.Item.RowId);
            if (count > 0) {
                return count;
            }
        }

        return 0;
    }

    internal static int GetCurrentItemLevel()
    {
        if (!AWC.PlayerState.IsLoaded) {
            return 0;
        }

        try {
            var equippedItems = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems);
            if (equippedItems == null) {
                return 0;
            }

            var totalItemLevel = 0u;
            var countedSlots = 12;
            var itemSheet = Svc.Data.GetExcelSheet<Item>();

            for (uint slotIndex = 0; slotIndex < 13; slotIndex++) {
                // Slot 5 is the soul crystal and is always excluded from average item level
                if (slotIndex == 5) {
                    continue;
                }

                var equippedItem = equippedItems->Items[slotIndex];
                var itemId = equippedItem.ItemId % 1000000;

                if (!itemSheet.TryGetRow(itemId, out var item)) {
                    continue;
                }

                var categoryId = item.ItemUICategory.RowId;
                if (IgnoreCategory.ContainsNullable(categoryId)) {
                    if (slotIndex == 0) {
                        // If main hand is ignored, offhand is also removed from the denominator
                        countedSlots -= 1;
                    }

                    countedSlots -= 1;
                    continue;
                }

                if (slotIndex == 0 && !CanHaveOffhand.ContainsNullable(categoryId)) {
                    // Jobs without offhand count main hand twice and skip the offhand slot
                    totalItemLevel += item.LevelItem.RowId;
                    slotIndex++;
                }

                totalItemLevel += item.LevelItem.RowId;
            }

            if (countedSlots == 0) {
                return 0;
            }

            return (int)(totalItemLevel / countedSlots);
        } catch (Exception) {
            return 0;
        }
    }
}
