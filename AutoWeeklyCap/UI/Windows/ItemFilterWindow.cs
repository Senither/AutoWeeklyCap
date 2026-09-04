using System.Threading;

using AutoWeeklyCap.Config;
using AutoWeeklyCap.Contracts.UI;
using AutoWeeklyCap.Helpers.MarketBoard;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

using Lumina.Excel.Sheets;

using Action = System.Action;
using Range = AutoWeeklyCap.UI.Helpers.Range;

namespace AutoWeeklyCap.UI.Windows;

public class ItemFilterWindow : ThemeWindow
{
    private static List<MarketBoardItem> FilteredItems = [];
    private static bool IsLoadingItems = false;
    private static bool HasLoadedItems = false;
    private static string SearchQuery = string.Empty;

    public ItemFilterWindow() : base("Item Filter##feedback-window")
    {
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(590, 400), MaximumSize = new Vector2(9999, 9999) };
    }

    public override void OnClose()
    {
        FilteredItems.Clear();
        IsLoadingItems = false;
        HasLoadedItems = false;

        Configuration.Save();
    }

    public override void Draw()
    {
        using var background = ImRaii.PushColor(ImGuiCol.ChildBg, Theme.BackgroundMedium);

        using (ImRaii.Child("###item-filter-content", new Vector2(0, ImGui.GetContentRegionAvail().Y), true)) {
            Card.Draw("Item Filters", DrawItemFilters, collapsible: false);
            Card.Draw("Blacklisted Items", DrawBlacklistedItems, collapsible: false);
            Card.Draw("Items to sell", DrawItemsToSell, collapsible: false);
        }
    }

    private static void DrawItemFilters()
    {
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X - 24f);

        var itemPriceType = AWC.Config.ItemFilters.ItemPriceType;
        if (ImGui.BeginCombo("##PreferredItemPriceType", itemPriceType.GetName())) {
            foreach (var item in Enum.GetValues<ItemPriceType>()) {
                if (ImGui.Selectable(item.GetName())) {
                    AWC.Config.ItemFilters.ItemPriceType = item;
                }
            }

            ImGui.EndCombo();
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Determines what type of price is used to get current marketboard");
            ImGui.Text("price for items being checked with Universalis.");
        });

        Card.Separator();

        Grid.DrawColumns("input-range-elements", [
            () =>
            {
                ImGui.Text("Gil Threshold");
                var gilThreshold = AWC.Config.ItemFilters.GilThreshold;
                if (Range.Draw("###Gil", ref gilThreshold, 0, 100_000)) {
                    AWC.Config.ItemFilters.GilThreshold = gilThreshold;
                }

                InformationTooltip.Draw(() =>
                {
                    ImGui.Text("Items that sell more than your set gil threshold on");
                    ImGui.Text("the marketboard according to Universalis will be");
                    ImGui.Text("excluded from being sold for gil.");
                });
            },
            () =>
            {
                ImGui.Text("Item Level Threshold");
                var itemLevelThreshold = AWC.Config.ItemFilters.ItemLevelThreshold;
                if (Range.Draw("###ItemThreshold", ref itemLevelThreshold, 0, Constants.CurrentMaxItemLevel)) {
                    AWC.Config.ItemFilters.ItemLevelThreshold = itemLevelThreshold;
                }

                InformationTooltip.Draw(() =>
                {
                    ImGui.Text("Items which has an item level at of above your preferred item level threshold");
                    ImGui.Text("will be excluded from being sold for gil, this includes gear, food, potions,");
                    ImGui.Text("materia, etc, that all has a base item level.");
                });
            }
        ], columnCount: 2, rowHeight: 46f);

        Card.Separator();

        ImGui.Text("Item types to exclude");

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Allows you to exclude entire categories of item types easily, an item type");
            ImGui.Text("that's excluded will never be included in the item filter.");
        });

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
    }

    private static void DrawBlacklistedItems()
    {
        HashSet<uint> blacklistedItems = AWC.Config.ItemFilters.BlacklistedItems;

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X - 24f);
        ImGui.InputTextWithHint("##blacklist-search", "Search items to blacklist", ref SearchQuery, 64);

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Items added to the blacklist will always be excluded from the filters, these");
            ImGui.Text("are items you'd like to always keep, regardless of what they sell for.");
        });

        if (!string.IsNullOrWhiteSpace(SearchQuery)) {
            IEnumerable<Item> matchingItems = Svc.Data.GetExcelSheet<Item>()
                .Where(item => item.RowId > 0 && item.Name.ToString().Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                .Where(item => !blacklistedItems.Contains(item.RowId))
                .Take(24);

            List<Action> searchResults = [];
            foreach (var item in matchingItems) {
                if (InventoryHelper.TryGetSheetItemFromItemId(item.RowId, out var itemObj)) {
                    searchResults.Add(() =>
                    {
                        ItemIcon.Draw(itemObj.Icon);
                        ImGui.Text($"{itemObj.Name}");

                        if (ImGuiEx.Ctrl && ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                            AWC.Config.ItemFilters.BlacklistedItems.Add(itemObj.RowId);
                        }

                        ImGuiEx.Tooltip("CTRL + Right click to add the item to the blacklist");
                    });
                }
            }

            if (searchResults.Count == 0 && SearchQuery.Length > 0) {
                ImGuiEx.TextCentered(Theme.TextMuted, "Found no items matching your search query");
            } else {
                Grid.DrawFixedWidth("search-results", searchResults, itemWidth: 260, rowHeight: 24f);
            }
        }

        Card.Separator();

        if (blacklistedItems.Count == 0) {
            ImGuiEx.TextCentered(Theme.TextMuted, "No items are currently blacklisted");
            return;
        }

        ImGui.Text($"Blacklisted Items ({blacklistedItems.Count})");
        ImGui.Spacing();

        List<Action> itemElements = [];
        foreach (var itemId in blacklistedItems) {
            if (InventoryHelper.TryGetSheetItemFromItemId(itemId, out var itemObj)) {
                itemElements.Add(() =>
                {
                    ItemIcon.Draw(itemObj.Icon);
                    ImGui.Text($"{itemObj.Name}");

                    if (ImGuiEx.Ctrl && ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                        AWC.Config.ItemFilters.BlacklistedItems.Remove(itemObj.RowId);
                    }

                    ImGuiEx.Tooltip("CTRL + Right click to remove the item from the blacklist");
                });
            }
        }

        Grid.DrawFixedWidth("items-blacklist", itemElements, itemWidth: 260, rowHeight: 24f);
    }

    private static void DrawItemsToSell()
    {
        Disabled.Draw(!PlayerHelper.IsValid, () =>
        {
            if (RightAlignedButton.Draw(IsLoadingItems ? "Loading..." : HasLoadedItems ? "Refresh Items" : "Load Items", offsetY: -2f)) {
                EnqueueLoadingItemsWithFilter();
            }
        });

        ImGui.Spacing();
        ImGui.Spacing();

        if (!HasLoadedItems) {
            ImGuiEx.TextCentered(Theme.TextMuted, "Click the \"Load Items\" button to see what items matches your current filter");
            return;
        }

        if (FilteredItems.Count == 0) {
            ImGuiEx.TextCentered(Theme.TextMuted, "Found no items to sell with your current filters");
            return;
        }

        List<Action> itemElements = [];
        foreach (var item in FilteredItems) {
            if (InventoryHelper.TryGetSheetItemFromItemId(item.ItemId, out var itemObj)) {
                itemElements.Add(() =>
                {
                    ItemIcon.Draw(itemObj.Icon);
                    ImGui.Text($"{itemObj.Name}");
                    ImGuiEx.Tooltip($"Selling for {item.GetPrice():N0}");
                });
            }
        }

        Grid.DrawFixedWidth("items", itemElements, itemWidth: 260, rowHeight: 24f);
    }

    private static void EnqueueLoadingItemsWithFilter()
    {
        if (IsLoadingItems) {
            return;
        }

        IsLoadingItems = true;
        uint serverId = Player.HomeWorld.RowId;

        var thread = new Thread(async void () =>
        {
            try {
                FilteredItems = (await MarketBoardHelper.GetFilteredMarketBoardItemsFromInventory(serverId))
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
                AWC.Log.Error($"[{nameof(ItemFilterWindow)}] Failed to fetch items from marketboard: {e.Message}\n{e.StackTrace}");
            } finally {
                IsLoadingItems = false;
                HasLoadedItems = true;
            }
        }) { IsBackground = true, Name = "AWC-ItemFilterLoader" };

        thread.Start();
    }
}
