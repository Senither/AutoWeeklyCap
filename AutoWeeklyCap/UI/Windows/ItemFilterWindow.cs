using AutoWeeklyCap.Contracts.UI;
using AutoWeeklyCap.Helpers.MarketBoard;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface.Windowing;

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
            AWC.TaskManager.Enqueue(async void () =>
            {
                try {
                    FilteredItems = await MarketBoardHelper.GetFilteredMarketBoardItemsFromInventory(Player.HomeWorld.RowId);
                } catch (Exception e) {
                    AWC.Log.Error("Failed to fetch items from marketboard", e);
                }
            });
        }

        var i = 0;

        foreach (var item in FilteredItems) {
            if (InventoryHelper.TryGetSheetItemFromItemId(item.ItemId, out var itemObj)) {
                ItemIcon.Draw(itemObj.Icon, 2f);

                ImGuiEx.Tooltip($"{itemObj.Name}: selling for {item.Price}");
            }

            if (++i % 10 == 0) {
                ImGui.NewLine();
            }
        }
    }
}
