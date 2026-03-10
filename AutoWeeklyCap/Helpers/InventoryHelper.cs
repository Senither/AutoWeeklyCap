using FFXIVClientStructs.FFXIV.Client.Game;

using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Helpers;

public static unsafe class InventoryHelper
{
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
}
