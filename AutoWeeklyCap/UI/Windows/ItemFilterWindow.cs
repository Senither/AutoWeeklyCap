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
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(550, 125), MaximumSize = new Vector2(550, 700) };

        Flags = ImGuiWindowFlags.AlwaysAutoResize;
    }

    public override void OnClose()
    {
        FilteredItems.Clear();
    }

    public override void Draw()
    {
        Card.Draw("Item Filters", () =>
        {
            var itemPriceType = AWC.Config.ItemFilters.ItemPriceType;
            if (ImGui.BeginCombo("##PreferredItemPriceType", itemPriceType.GetName())) {
                foreach (var item in Enum.GetValues<ItemPriceType>()) {
                    if (ImGui.Selectable(item.GetName())) {
                        AWC.Config.ItemFilters.ItemPriceType = item;
                    }
                }

                ImGui.EndCombo();
            }

            Card.Separator();

            Grid.DrawColumns("input-range-elements", [
                () =>
                {
                    ImGui.Text("Gil Threshold");
                    var gilThreshold = AWC.Config.ItemFilters.GilThreshold;
                    if (Range.Draw("###Gil", ref gilThreshold, 0, 100_000)) {
                        AWC.Config.ItemFilters.GilThreshold = gilThreshold;
                    }
                },
                () =>
                {
                    ImGui.Text("Item Level Threshold");
                    var itemLevelThreshold = AWC.Config.ItemFilters.ItemLevelThreshold;
                    if (Range.Draw("###ItemThreshold", ref itemLevelThreshold, 0, Constants.CurrentMaxItemLevel)) {
                        AWC.Config.ItemFilters.ItemLevelThreshold = itemLevelThreshold;
                    }
                }
            ], columnCount: 2, rowHeight: 46f);

            Card.Separator();

            ImGui.Text("Items to exclude");

            Grid.DrawColumns("input-checkbox-elements", [
                () =>
                {
                    var excludeMateria = AWC.Config.ItemFilters.ExcludeMateria;
                    if (ImGui.Checkbox("Materia", ref excludeMateria)) {
                        AWC.Config.ItemFilters.ExcludeMateria = excludeMateria;
                    }
                },
                () =>
                {
                    var excludeFood = AWC.Config.ItemFilters.ExcludeFood;
                    if (ImGui.Checkbox("Food", ref excludeFood)) {
                        AWC.Config.ItemFilters.ExcludeFood = excludeFood;
                    }
                },
                () =>
                {
                    var excludePotions = AWC.Config.ItemFilters.ExcludePotions;
                    if (ImGui.Checkbox("Potions", ref excludePotions)) {
                        AWC.Config.ItemFilters.ExcludePotions = excludePotions;
                    }
                },
                () =>
                {
                    var excludeDyes = AWC.Config.ItemFilters.ExcludeDyes;
                    if (ImGui.Checkbox("Dyes", ref excludeDyes)) {
                        AWC.Config.ItemFilters.ExcludeDyes = excludeDyes;
                    }
                }
            ], columnCount: 4, rowHeight: 26f);
        }, collapsible: false);

        Card.Draw("Items to sell", () =>
        {
            if (RightAlignedButton.Draw(FilteredItems.Count == 0 ? "Load Items" : "Refresh Items", offsetY: -2f)) {
                EnqueueLoadingItemsWithFilter();
            }

            ImGui.Spacing();
            ImGui.Spacing();

            List<Action> itemElements = [];
            foreach (var item in FilteredItems) {
                if (InventoryHelper.TryGetSheetItemFromItemId(item.ItemId, out var itemObj)) {
                    itemElements.Add(() =>
                    {
                        ItemIcon.Draw(itemObj.Icon);
                        ImGui.Text($"{itemObj.Name}");
                        ImGuiEx.Tooltip($"Selling for {item.GetPrice(itemObj.CanBeHq)}\nUI Category: {itemObj.ItemUICategory.RowId}\nItem Level: {itemObj.LevelItem.RowId}\nItem ID: {itemObj.RowId}");
                    });
                }
            }

            Grid.DrawColumns("items", itemElements, columnCount: 2, rowHeight: 24f);
        }, collapsible: false);
    }

    private static void EnqueueLoadingItemsWithFilter()
    {
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
}
