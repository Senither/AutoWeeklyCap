using Dalamud.Game.Inventory;

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

    internal static uint GetEmptySlotsInBag()
    {
        return InventoryManager.Instance()->GetEmptySlotsInBag();
    }

    internal static bool IsAtleastOneArmoryChestSlotFull()
    {
        try {
            foreach (var inventoryType in Enum.GetValues<GameInventoryType>()) {
                if (!inventoryType.ToString().Contains("Armory", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                ReadOnlySpan<GameInventoryItem> items = Svc.GameInventory.GetInventoryItems(inventoryType);
                if (items.Length == 0) {
                    continue;
                }

                var isContainerFull = true;

                foreach (var item in items) {
                    if (item.IsEmpty) {
                        isContainerFull = false;
                        break;
                    }
                }

                if (isContainerFull) {
                    return true;
                }
            }

            return false;
        } catch (Exception) {
            return false;
        }
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

    internal static List<GameInventoryItem> GetSaddleBagItems()
    {
        ReadOnlySpan<GameInventoryItem> bag1 = Svc.GameInventory.GetInventoryItems(GameInventoryType.SaddleBag1);
        ReadOnlySpan<GameInventoryItem> bag2 = Svc.GameInventory.GetInventoryItems(GameInventoryType.SaddleBag2);
        ReadOnlySpan<GameInventoryItem> premium1 = Svc.GameInventory.GetInventoryItems(GameInventoryType.PremiumSaddleBag1);
        ReadOnlySpan<GameInventoryItem> premium2 = Svc.GameInventory.GetInventoryItems(GameInventoryType.PremiumSaddleBag2);

        var combined = new List<GameInventoryItem>(bag1.Length + bag2.Length + premium1.Length + premium2.Length);

        combined.AddRange(bag1);
        combined.AddRange(bag2);
        combined.AddRange(premium1);
        combined.AddRange(premium2);

        return combined;
    }

    internal static List<GameInventoryItem> GetPlayerInventoryItems()
    {
        ReadOnlySpan<GameInventoryItem> inv1 = Svc.GameInventory.GetInventoryItems(GameInventoryType.Inventory1);
        ReadOnlySpan<GameInventoryItem> inv2 = Svc.GameInventory.GetInventoryItems(GameInventoryType.Inventory2);
        ReadOnlySpan<GameInventoryItem> inv3 = Svc.GameInventory.GetInventoryItems(GameInventoryType.Inventory3);
        ReadOnlySpan<GameInventoryItem> inv4 = Svc.GameInventory.GetInventoryItems(GameInventoryType.Inventory4);

        var combined = new List<GameInventoryItem>(inv1.Length + inv2.Length + inv3.Length + inv4.Length);

        combined.AddRange(inv1);
        combined.AddRange(inv2);
        combined.AddRange(inv3);
        combined.AddRange(inv4);

        return combined;
    }

    private static bool TryGetSheetItemFromInventoryItem(InventoryItem container, out Item item)
    {
        if (!Svc.Data.GetExcelSheet<Item>().TryGetRow(container.ItemId % 1000000, out item)) {
            return false;
        }

        return item.RowId > 0;
    }
}
