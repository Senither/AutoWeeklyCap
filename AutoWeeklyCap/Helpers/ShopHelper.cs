using ECommons.ExcelServices;
using ECommons.UIHelpers.AddonMasterImplementations;

using FFXIVClientStructs.FFXIV.Client.UI;

using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Helpers;

public static class ShopHelper
{
    public sealed record ShopItemMatch(int Index, Item Item, ItemSlot Slot, ItemType Type);

    internal static IEnumerable<AddonMaster.ShopExchangeCurrency.ShopItemInfo> GetAllShopExchangeCurrencyItems(AddonMaster.ShopExchangeCurrency shop)
    {
        var defaultItems = ReadShopExchangeCurrencyItems(shop, 1066, 456, 1310);
        var fallbackItems = ReadShopExchangeCurrencyItems(shop, 1064, 454, 1308);

        // Some client versions expose this data 2 AtkValue slots earlier.
        return fallbackItems.Count > defaultItems.Count ? fallbackItems : defaultItems;
    }

    private static unsafe List<AddonMaster.ShopExchangeCurrency.ShopItemInfo> ReadShopExchangeCurrencyItems(
        AddonMaster.ShopExchangeCurrency shop,
        int itemIdStart,
        int costStart,
        int indexStart
    )
    {
        var items = new List<AddonMaster.ShopExchangeCurrency.ShopItemInfo>();

        for (var i = 0; i < shop.NumEntries; i++) {
            var itemId = shop.Addon->AtkValues[itemIdStart + i].UInt;
            if (itemId == 0u) {
                continue;
            }

            // @formatter:off
            items.Add(new AddonMaster.ShopExchangeCurrency.ShopItemInfo(shop)
            {
                ItemId = itemId,
                CostAmount = shop.Addon->AtkValues[costStart + i].UInt,
                Index = shop.Addon->AtkValues[indexStart + i].UInt,
            });
            // @formatter:on
        }

        return items;
    }

    internal static unsafe ShopItemMatch? GetMatchingShopItem(AddonShop* addonShop, ItemSlot expectedSlot, ItemType expectedType, PlayerJob job, int requiredLevel)
    {
        AWC.Log.Debug($"{nameof(ShopHelper)}: Starting shop match with (Slot: {expectedSlot}, Type: {expectedType}, Job: {job}, RequiredLevel: {requiredLevel})");

        var shop = new AddonMaster.Shop(addonShop);
        if (shop.NumEntries <= 0) {
            return null;
        }

        for (var i = 0; i < shop.NumEntries; i++) {
            AddonMaster.Shop.ShopItemInfo shopItem = shop.ShopItems[i];
            if (!InventoryHelper.TryGetSheetItemFromItemId(shopItem.ItemId, out Item item)) {
                continue;
            }

            if (item.LevelEquip != requiredLevel) {
                continue;
            }

            if (!item.ClassJobCategory.Value.IsJobInCategory((Job)job)) {
                continue;
            }

            ItemSlot? slot = ItemSlotExtensions.FromItem(item);
            if (slot == null) {
                AWC.Log.Debug($"{nameof(ShopHelper)}: Shop item scan [{i}]: {item.Name} (#{item.RowId}) has no recognized equip slot | EquipSlotCategory: {item.EquipSlotCategory.RowId} | ItemUICategory: {item.ItemUICategory.RowId}");
                continue;
            }

            if (!expectedSlot.IsMatch(slot.Value)) {
                continue;
            }

            ItemType type = ItemTypeExtensions.FromItem(item);
            if (!slot.Value.IsWeapon() && expectedType != type) {
                continue;
            }

            return new ShopItemMatch(i, item, slot.Value, type);
        }

        return null;
    }
}
