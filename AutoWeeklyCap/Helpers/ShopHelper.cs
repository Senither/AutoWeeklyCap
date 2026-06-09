using ECommons.ExcelServices;

using FFXIVClientStructs.FFXIV.Client.UI;

using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Helpers;

public static class ShopHelper
{
    public sealed record ShopItemMatch(int Index, uint ItemId, string Name, ItemSlot Slot, ItemType Type, bool CanEquipCurrentJob);

    private static Dictionary<string, uint>? _itemNameToRowId;
    private static Dictionary<string, uint>? _normalizedItemNameToRowId;

    internal static unsafe ShopItemMatch? GetMatchingShopItem(AddonShop* addonShop, ItemSlot expectedSlot, ItemType expectedType, PlayerJob job, int requiredLevel)
    {
        var buyList = addonShop->BuyList;
        if (buyList == null) {
            return null;
        }

        var listLength = buyList->ListLength;
        if (listLength <= 0) {
            return null;
        }

        for (var i = 0; i < listLength; i++) {
            var rawItemName = buyList->GetItemLabel(i).ToString();
            var itemName = NormalizeShopItemLabel(rawItemName);
            if (string.IsNullOrWhiteSpace(itemName)) {
                continue;
            }

            if (!TryGetItemCandidatesFromShopLabel(itemName, out var candidates, out var usedPartialMatch)) {
                AWC.Log.Debug($"{nameof(ShopHelper)}: Shop item scan [{i}]: raw=\"{rawItemName}\" | normalized=\"{itemName}\" could not be resolved in Item sheet");
                continue;
            }

            if (usedPartialMatch) {
                AWC.Log.Debug($"{nameof(ShopHelper)}: Shop item scan [{i}]: raw=\"{rawItemName}\" | normalized=\"{itemName}\" matched {candidates.Count} candidate item(s) via partial name lookup");
            }

            foreach (var item in candidates) {
                var slot = ItemSlotExtensions.FromItem(item);

                var canEquipCurrentJob = item.ClassJobCategory.Value.IsJobInCategory((Job)job);
                var itemRequiredLevel = item.LevelEquip;

                if (slot == null && expectedSlot.IsWeapon() && canEquipCurrentJob && itemRequiredLevel == requiredLevel) {
                    slot = InferWeaponSlotFromItem(item, expectedSlot);
                }

                if (slot == null) {
                    AWC.Log.Debug($"{nameof(ShopHelper)}: Shop item scan [{i}]: {item.Name} (#{item.RowId}) has no recognized equip slot | EquipSlotCategory: {item.EquipSlotCategory.RowId} | ItemUICategory: {item.ItemUICategory.RowId}");
                    continue;
                }

                var type = ItemTypeExtensions.FromItem(item);

                var slotMatch = expectedSlot.IsMatch(slot.Value);
                var typeMatch = expectedSlot.IsWeapon() || expectedType == type;
                var levelMatch = itemRequiredLevel == requiredLevel;

                var isMatch = slotMatch && typeMatch && canEquipCurrentJob && levelMatch;

                AWC.Log.Debug($"{nameof(ShopHelper)}: Shop item scan [{i}]: {item.Name} (#{item.RowId}) | Slot: {slot} | Type: {type} | CanEquip({job}): {canEquipCurrentJob} | ReqLevel: {itemRequiredLevel} (Wanted: {requiredLevel}) | Match: {isMatch}");

                if (isMatch) {
                    return new ShopItemMatch(i, item.RowId, item.Name.ToString(), slot.Value, type, canEquipCurrentJob);
                }
            }
        }

        return null;
    }

    private static ItemSlot? InferWeaponSlotFromItem(Item item, ItemSlot expectedSlot)
    {
        if (item.EquipSlotCategory.RowId is 1 or 2) {
            return item.EquipSlotCategory.RowId == 1 ? ItemSlot.MainHand : ItemSlot.OffHand;
        }

        var itemUiCategoryName = item.ItemUICategory.Value.Name.ToString();
        if (!string.IsNullOrWhiteSpace(itemUiCategoryName)) {
            // See: https://ffxiv.consolegameswiki.com/wiki/Shield
            var isShield = itemUiCategoryName.Contains("shield", StringComparison.OrdinalIgnoreCase) ||
                           itemUiCategoryName.Contains("off-hand", StringComparison.OrdinalIgnoreCase) ||
                           itemUiCategoryName.Contains("offhand", StringComparison.OrdinalIgnoreCase) ||
                           itemUiCategoryName.Contains("hoplon", StringComparison.OrdinalIgnoreCase) ||
                           itemUiCategoryName.Contains("buckler", StringComparison.OrdinalIgnoreCase) ||
                           itemUiCategoryName.Contains("scutum", StringComparison.OrdinalIgnoreCase);

            if (isShield) {
                return ItemSlot.OffHand;
            }
        }

        return expectedSlot == ItemSlot.OffHand ? null : ItemSlot.MainHand;
    }

    private static bool TryGetItemCandidatesFromShopLabel(string shopLabel, out List<Item> items, out bool usedPartialMatch)
    {
        items = [];
        usedPartialMatch = false;

        if (string.IsNullOrWhiteSpace(shopLabel)) {
            return false;
        }

        var cleanedLabel = NormalizeShopItemLabel(shopLabel);
        if (string.IsNullOrEmpty(cleanedLabel)) {
            return false;
        }

        var sheet = Svc.Data.GetExcelSheet<Item>();

        _itemNameToRowId ??= BuildItemNameIndex(sheet);
        _normalizedItemNameToRowId ??= BuildNormalizedItemNameIndex(sheet);

        if (_itemNameToRowId.TryGetValue(cleanedLabel, out var rowId) ||
            _normalizedItemNameToRowId.TryGetValue(cleanedLabel, out rowId)) {
            if (!sheet.TryGetRow(rowId, out var exactItem)) {
                return false;
            }

            items.Add(exactItem);
            return true;
        }

        if (!cleanedLabel.Contains("...", StringComparison.Ordinal)) {
            return false;
        }

        var truncatedPrefix = cleanedLabel.Replace("...", string.Empty, StringComparison.Ordinal).Trim();
        if (truncatedPrefix.Length < 4) {
            return false;
        }

        var seen = new HashSet<uint>();
        foreach (var candidate in _normalizedItemNameToRowId) {
            if (!candidate.Key.StartsWith(truncatedPrefix, StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            if (!seen.Add(candidate.Value)) {
                continue;
            }

            if (!sheet.TryGetRow(candidate.Value, out var matchedItem)) {
                continue;
            }

            items.Add(matchedItem);
        }

        usedPartialMatch = items.Count > 0;
        return usedPartialMatch;
    }

    private static Dictionary<string, uint> BuildItemNameIndex(ExcelSheet<Item> itemSheet)
    {
        var map = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in itemSheet) {
            if (item.RowId == 0) {
                continue;
            }

            var name = item.Name.ToString();
            if (string.IsNullOrWhiteSpace(name) || map.ContainsKey(name)) {
                continue;
            }

            map[name] = item.RowId;
        }

        return map;
    }

    private static Dictionary<string, uint> BuildNormalizedItemNameIndex(ExcelSheet<Item> itemSheet)
    {
        var map = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in itemSheet) {
            if (item.RowId == 0) {
                continue;
            }

            var normalizedName = NormalizeShopItemLabel(item.Name.ToString());
            if (string.IsNullOrWhiteSpace(normalizedName) || map.ContainsKey(normalizedName)) {
                continue;
            }

            map[normalizedName] = item.RowId;
        }

        return map;
    }

    private static string NormalizeShopItemLabel(string? rawLabel)
    {
        if (string.IsNullOrWhiteSpace(rawLabel)) {
            return string.Empty;
        }

        Span<char> buffer = stackalloc char[rawLabel.Length];
        var count = 0;

        foreach (var c in rawLabel) {
            if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c is '\'' or '-' or '(' or ')' or '.' or ':') {
                buffer[count++] = c;
            }
        }

        if (count == 0) {
            return string.Empty;
        }

        var stripped = new string(buffer[..count]);

        // Normalize repeated spaces introduced while stripping icon/control payload.
        var normalized = string.Join(' ', stripped.Split(' ', StringSplitOptions.RemoveEmptyEntries)).Trim();

        // Shop payload formatting can leak uppercase wrapper tokens around the true item name.
        while (normalized.StartsWith("HI", StringComparison.Ordinal)) {
            normalized = normalized[2..].TrimStart();
        }

        while (normalized.EndsWith("IH", StringComparison.Ordinal)) {
            normalized = normalized[..^2].TrimEnd();
        }

        return normalized;
    }
}
