using System.Text;

using AutoWeeklyCap.UI.Helpers;

using ECommons.UIHelpers.AddonMasterImplementations;

using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.DeveloperToolbox;

internal static class AddonDebugActionsSection
{
    internal static unsafe void Draw()
    {
        DebugButton.Draw("ShopExchangeCurrency", DrawShopExchangeCurrency, false);
        DebugButton.Draw("Shop", DrawShop);
        DebugButton.Draw("SelectString", DrawSelectString);
        DebugButton.Draw("SelectIconString", DrawSelectIconString);
        DebugButton.Draw("SelectYesno", DrawSelectYesno);
    }

    private static unsafe string DrawShopExchangeCurrency(AtkUnitBase* addon)
    {
        StringBuilder builder = new StringBuilder();
        IEnumerable<AddonMaster.ShopExchangeCurrency.ShopItemInfo> items = ShopHelper.GetAllShopExchangeCurrencyItems(new AddonMaster.ShopExchangeCurrency(addon));

        foreach (AddonMaster.ShopExchangeCurrency.ShopItemInfo item in items) {
            builder.Append($"Index={item.Index}, ItemId={item.ItemId}, CostAmount={item.CostAmount}");

            if (InventoryHelper.TryGetSheetItemFromItemId(item.ItemId, out var itemObj)) {
                builder.Append($", ItemName={itemObj.Name}");
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }

    private static unsafe string DrawShop(AtkUnitBase* addon)
    {
        StringBuilder builder = new StringBuilder();

        foreach (AddonMaster.Shop.ShopItemInfo item in new AddonMaster.Shop(addon).ShopItems) {
            builder.Append($"ItemId={item.ItemId}, CostAmount={item.CostAmount}");

            if (InventoryHelper.TryGetSheetItemFromItemId(item.ItemId, out var itemObj)) {
                builder.Append($", ItemName={itemObj.Name}");
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }

    private static unsafe string DrawSelectString(AtkUnitBase* addon)
    {
        StringBuilder builder = new StringBuilder();
        AddonMaster.SelectString select = new AddonMaster.SelectString(addon);

        builder.AppendLine($"Text:");
        builder.AppendLine(select.Text.Length == 0 ? "<empty>" : select.Text);
        builder.AppendLine();
        builder.AppendLine("Entries:");

        foreach (var item in select.Entries) {
            builder.Append($"Index={item.Index}, Text={item.Text}\n");
        }

        return builder.ToString();
    }

    private static unsafe string DrawSelectIconString(AtkUnitBase* addon)
    {
        StringBuilder builder = new StringBuilder();
        AddonMaster.SelectIconString select = new AddonMaster.SelectIconString(addon);

        builder.AppendLine("Entries:");

        foreach (var item in select.Entries) {
            builder.Append($"Index={item.Index}, Text={item.Text}\n");
        }

        return builder.ToString();
    }

    private static unsafe string DrawSelectYesno(AtkUnitBase* addon)
    {
        StringBuilder builder = new StringBuilder();
        AddonMaster.SelectYesno select = new AddonMaster.SelectYesno(addon);

        builder.AppendLine($"Text:");
        builder.AppendLine(select.Text.Length == 0 ? "<empty>" : select.Text);
        builder.AppendLine();
        builder.AppendLine($"YesButton: {select.Addon->YesButton->ButtonTextNode->GetText()}");
        builder.AppendLine($"NoButton: {select.Addon->NoButton->ButtonTextNode->GetText()}");

        if (select.ThirdButton != null && select.ThirdButton->ButtonTextNode->GetText().Length > 0) {
            builder.AppendLine($"ThirdButton: {select.ThirdButton->ButtonTextNode->GetText()}");
        }

        return builder.ToString();
    }
}
