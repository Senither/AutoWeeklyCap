using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Helpers;

public static unsafe class InventoryHelper
{
    internal static bool CanRepair() => CanRepair(AutoWeeklyCap.Config.RepairPercentage);
    internal static bool CanRepair(uint percent) => (LowestEquippedItem().Condition / 300f) <= percent;

    internal static InventoryItem LowestEquippedItem()
    {
        var equippedItems = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems);

        uint lowestCondition = 999999;
        uint lowestItem = 0;

        for (uint i = 0; i < 13; i++)
        {
            var item = equippedItems->Items[i];
            if (lowestCondition > item.Condition)
            {
                lowestItem = i;
                lowestCondition = item.Condition;
            }
        }

        return equippedItems->Items[lowestItem];
    }
}
