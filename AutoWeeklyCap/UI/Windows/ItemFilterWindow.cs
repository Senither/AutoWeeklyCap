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
        var itemPriceType = AWC.Config.ItemFilters.ItemPriceType;
        if (ImGui.BeginCombo("##PreferredItemPriceType", itemPriceType.GetName())) {
            foreach (var item in Enum.GetValues<ItemPriceType>()) {
                if (ImGui.Selectable(item.GetName())) {
                    AWC.Config.ItemFilters.ItemPriceType = item;
                }
            }

            ImGui.EndCombo();
        }

        DrawGrid([
            () =>
            {
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 3);

                var gilThreshold = AWC.Config.ItemFilters.GilThreshold;
                if (Range.Draw("###Gil", ref gilThreshold, 0, 100_000)) {
                    AWC.Config.ItemFilters.GilThreshold = gilThreshold;
                }
            },
            () =>
            {
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 3);
                var itemLevelThreshold = AWC.Config.ItemFilters.ItemLevelThreshold;
                if (Range.Draw("###ItemThreshold", ref itemLevelThreshold, 0, Constants.CurrentMaxItemLevel)) {
                    AWC.Config.ItemFilters.ItemLevelThreshold = itemLevelThreshold;
                }
            }
        ], colum: 2, height: 50);

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

            DrawGrid(itemElements, colum: 2, height: 28);
        }, collapsible: false);
    }

    private static void DrawGrid(List<Action> elements, int colum = 2, int height = 40)
    {
        Vector2 start = ImGui.GetCursorScreenPos();
        float width = (ImGui.GetContentRegionAvail().X / colum) - ImGui.GetStyle().ItemSpacing.X;

        int currentRow = 0;

        for (var i = 0; i < elements.Count; i++) {
            ImGui.SetCursorScreenPos(start + new Vector2(i % colum * width, currentRow * height));

            elements[i].Invoke();

            if ((i + 1) % colum == 0) {
                currentRow++;
            }
        }

        ImGui.SetCursorScreenPos(start + new Vector2(0, currentRow * height));
        ImGui.Dummy(Vector2.Zero);
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
