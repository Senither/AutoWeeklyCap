using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Helpers;

public static unsafe class InventoryHelper
{
    private static readonly uint[] CanHaveOffhand = [2, 6, 8, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32];
    private static readonly uint[] IgnoreCategory = [105];

    internal static int GetItemCount(uint itemId)
    {
        return InventoryManager.Instance()->GetInventoryItemCount(itemId);
    }

    internal static void UseItem(uint itemId)
    {
        ActionManager.Instance()->UseAction(ActionType.Item, itemId, extraParam: 65535);
    }

    internal static bool CanRepair()
    {
        return CanRepair(AWC.Config.RepairPercentage);
    }

    internal static bool CanRepair(uint percent)
    {
        return (GetLowestConditionEquippedItem().Condition / 300f) <= percent;
    }

    internal static InventoryItem GetLowestConditionEquippedItem()
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
            var uiState = UIState.Instance();
            if (uiState == null) {
                return 0;
            }

            return uiState->CurrentItemLevel;
        } catch (Exception) {
            return 0;
        }
    }

    internal static (Item?, ItemSlot) GetLowestEquippedItemLevelItem()
    {
        if (!AWC.PlayerState.IsLoaded) {
            return default;
        }

        try {
            var equippedItems = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems);
            if (equippedItems == null) {
                return default;
            }

            var canUseOffhand = true;
            if (TryGetSheetItemFromInventoryItem(equippedItems->Items[ItemSlot.MainHand.GetSlot()], out var mainHandItem)) {
                canUseOffhand = CanHaveOffhand.ContainsNullable(mainHandItem.ItemUICategory.RowId);
            }

            Item? lowestItem = null;
            var lowestSlot = default(ItemSlot);
            var lowestItemLevel = uint.MaxValue;

            foreach (var slot in Enum.GetValues<ItemSlot>()) {
                if (slot == ItemSlot.OffHand && !canUseOffhand) {
                    continue;
                }

                var equippedItem = equippedItems->Items[slot.GetSlot()];
                if (equippedItem.ItemId == 0 || !TryGetSheetItemFromInventoryItem(equippedItem, out var item)) {
                    return (null, slot);
                }

                var itemLevel = item.LevelItem.RowId;
                if (itemLevel >= lowestItemLevel) {
                    continue;
                }

                lowestItem = item;
                lowestSlot = slot;
                lowestItemLevel = itemLevel;
            }

            return (lowestItem, lowestSlot);
        } catch (Exception) {
            return default;
        }
    }

    private static bool TryGetSheetItemFromInventoryItem(InventoryItem container, out Item item)
    {
        if (!Svc.Data.GetExcelSheet<Item>().TryGetRow(container.ItemId % 1000000, out item)) {
            return false;
        }

        return item.RowId > 0;
    }
}
