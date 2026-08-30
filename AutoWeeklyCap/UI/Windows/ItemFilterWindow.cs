using AutoWeeklyCap.Contracts.UI;
using AutoWeeklyCap.Helpers.MarketBoard;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface.Windowing;

using Range = AutoWeeklyCap.UI.Helpers.Range;

namespace AutoWeeklyCap.UI.Windows;

public class ItemFilterWindow : ThemeWindow
{
    private static List<MarketBoardItem> FilteredItems = [];

    public ItemFilterWindow() : base("Item Filter##feedback-window")
    {
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(550, 125), MaximumSize = new Vector2(550, 9999) };

        Flags = ImGuiWindowFlags.AlwaysAutoResize;
    }

    public override void OnClose()
    {
        FilteredItems.Clear();
    }

    public override void Draw()
    {
        if (ImGui.Button("Load items from marketboard")) {
            AWC.TaskManager.Insert(async void () =>
            {
                try {
                    FilteredItems = (await MarketBoardHelper.GetFilteredMarketBoardItemsFromInventory())
                        .Select(marketBoardItem =>
                        {
                            var hasSheetItem = InventoryHelper.TryGetSheetItemFromItemId(marketBoardItem.ItemId, out var sheetItem);
                            return new { MarketBoardItem = marketBoardItem, SheetItem = sheetItem, HasSheetItem = hasSheetItem };
                        })
                        .OrderBy(item => item.HasSheetItem ? item.SheetItem.ItemUICategory.RowId : uint.MaxValue)
                        .ThenBy(item => item.HasSheetItem ? item.SheetItem.Name.ToString() : string.Empty, StringComparer.Ordinal)
                        .Select(item => item.MarketBoardItem)
                        .ToList();
                } catch (Exception e) {
                    AWC.Log.Error("Failed to fetch items from marketboard", e);
                }
            });
        }

        var gilThreshold = AWC.Config.ItemFilters.GilThreshold;
        if (Range.Draw("Gil Threshold", ref gilThreshold, 0, 100_000)) {
            AWC.Config.ItemFilters.GilThreshold = gilThreshold;
        }

        var itemLevelThreshold = AWC.Config.ItemFilters.ItemLevelThreshold;
        if (Range.Draw("Item Level Threshold", ref itemLevelThreshold, 0, Constants.CurrentMaxItemLevel)) {
            AWC.Config.ItemFilters.ItemLevelThreshold = itemLevelThreshold;
        }

        var excludeMateria = AWC.Config.ItemFilters.ExcludeMateria;
        if (ImGui.Checkbox("Exclude Materia", ref excludeMateria)) {
            AWC.Config.ItemFilters.ExcludeMateria = excludeMateria;
        }

        var excludeFood = AWC.Config.ItemFilters.ExcludeFood;
        if (ImGui.Checkbox("Exclude Food", ref excludeFood)) {
            AWC.Config.ItemFilters.ExcludeFood = excludeFood;
        }

        var excludePotions = AWC.Config.ItemFilters.ExcludePotions;
        if (ImGui.Checkbox("Exclude Potions", ref excludePotions)) {
            AWC.Config.ItemFilters.ExcludePotions = excludePotions;
        }

        var excludeDyes = AWC.Config.ItemFilters.ExcludeDyes;
        if (ImGui.Checkbox("Exclude Dyes", ref excludeDyes)) {
            AWC.Config.ItemFilters.ExcludeDyes = excludeDyes;
        }

        ImGui.Spacing();

        var i = 0;

        foreach (var item in FilteredItems) {
            if (InventoryHelper.TryGetSheetItemFromItemId(item.ItemId, out var itemObj)) {
                ItemIcon.Draw(itemObj.Icon, 2f);

                ImGuiEx.Tooltip($"{itemObj.Name}: selling for {item.GetPrice(itemObj.CanBeHq)}\nUI Category: {itemObj.ItemUICategory.RowId}\nItem Level: {itemObj.LevelItem.RowId}\nItem ID: {itemObj.RowId}");
            }

            if (++i % 10 == 0) {
                ImGui.NewLine();
            }
        }
    }
}
