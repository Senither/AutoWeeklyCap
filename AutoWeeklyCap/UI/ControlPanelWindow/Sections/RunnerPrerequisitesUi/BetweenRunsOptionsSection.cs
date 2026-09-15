using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface;

using Lumina.Excel.Sheets;

using Range = AutoWeeklyCap.UI.Helpers.Range;
using TomestoneItem = AutoWeeklyCap.Config.TomestoneItem;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.RunnerPrerequisitesUi;

internal static class BetweenRunsOptionsSection
{
    private const float Spacing = 4f;

    internal static void Draw()
    {
        DrawRepairGearOptions();
        DrawAutoExtractOptions();
        DrawTripleTriadCardOptions();
        DrawMoveItemsBetweenInventoryAndSaddleBagOptions();
        DrawAutoSellWorthlessItemOptions();
        DrawAutoSpendTomestoneOptions();
    }

    private static void DrawRepairGearOptions()
    {
        bool repairStatus = AWC.Config.Repair;
        if (ImGui.Checkbox("Repair Gear", ref repairStatus)) {
            AWC.Config.Repair = repairStatus;
        }

        Disabled.Draw(!AWC.Config.Repair, () =>
        {
            ImGui.SameLine();
            if (ImGui.RadioButton("Self", AWC.Config.RepairSelf)) {
                AWC.Config.RepairSelf = true;
            }

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("Will use Dark Matter to Self Repair (Requires Leveled Crafters!)");
                ImGui.Text("If self repair is not possible NPC repairs will be used instead");
            });

            ImGui.SameLine();
            if (ImGui.RadioButton("City NPC", !AWC.Config.RepairSelf)) {
                AWC.Config.RepairSelf = false;
            }

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("Will teleport to your grand company and use gil to repair your gear");

                ImGui.Text("Requires ");
                StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
                ImGui.Text(" and ");
                StatusText.Draw(VNavMeshIPC.IsEnabled, "VNavMesh");
                ImGui.Text(" to be enabled");
            });

            ImGui.Text("Trigger @");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);

            uint autoRepairPercentage = AWC.Config.RepairPercentage;
            if (Range.Draw("##Repair@", ref autoRepairPercentage, 1, 99, "%d%%")) {
                AWC.Config.RepairPercentage = Math.Min(100, Math.Max(1, autoRepairPercentage));
            }

            ImGui.Spacing();
        });
    }

    private static void DrawAutoExtractOptions()
    {
        bool autoExtract = AWC.Config.Extract;
        if (ImGui.Checkbox("Extract Materia", ref autoExtract)) {
            AWC.Config.Extract = autoExtract;
        }

        Disabled.Draw(!AWC.Config.Extract, () =>
        {
            ImGui.SameLine(0, 10);
            if (ImGui.RadioButton("Equipped", !AWC.Config.ExtractAll)) {
                AWC.Config.ExtractAll = false;
            }

            ImGui.SameLine(0, 5);
            if (ImGui.RadioButton("All", AWC.Config.ExtractAll)) {
                AWC.Config.ExtractAll = true;
            }
        });
    }

    private static void DrawTripleTriadCardOptions()
    {
        bool learnTtCards = AWC.Config.LearnTripleTriadCards;
        if (ImGui.Checkbox("Learn Triple Triad Cards", ref learnTtCards)) {
            AWC.Config.LearnTripleTriadCards = learnTtCards;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled the runner will use any Triple Triad cards");
            ImGui.Text("in your inventory that you haven't already learned.");
        });

        ImGui.SameLine();

        bool sellTtCards = AWC.Config.SellTripleTriadCards;
        if (ImGui.Checkbox("Sell Triple Triad Cards", ref sellTtCards)) {
            AWC.Config.SellTripleTriadCards = sellTtCards;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled and you have at least the minimum amount of Triple Triad");
            ImGui.Text("cards in your inventory, the runner will teleport to The Golden");
            ImGui.Text("Saucer and trade in the cards for MGP.");

            ImGui.Text("Requires ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" and ");
            StatusText.Draw(VNavMeshIPC.IsEnabled, "VNavMesh");
            ImGui.Text(" to be enabled");
        });

        Disabled.Draw(!sellTtCards, () =>
        {
            ImGui.Text("Sell TT cards @");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(80 * ImGuiHelpers.GlobalScale);

            uint sellTripleTriadCardsAtMinimum = AWC.Config.SellTripleTriadCardsAtMinimum;
            if (Range.Draw("##SellTTCards@", ref sellTripleTriadCardsAtMinimum, 1, 99)) {
                AWC.Config.SellTripleTriadCardsAtMinimum = sellTripleTriadCardsAtMinimum;
            }

            ImGui.SameLine();
            ImGui.Text("cards");

            ImGui.Spacing();
        });
    }

    private static void DrawMoveItemsBetweenInventoryAndSaddleBagOptions()
    {
        bool moveDuplicateItemsFromInventoryToSaddlebag = AWC.Config.MoveDuplicateItemsFromInventoryToSaddlebag;
        if (ImGui.Checkbox("Move duplicated stackable items to saddlebag", ref moveDuplicateItemsFromInventoryToSaddlebag)) {
            AWC.Config.MoveDuplicateItemsFromInventoryToSaddlebag = moveDuplicateItemsFromInventoryToSaddlebag;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When this option is enabled, the runner will look for items that are both");
            ImGui.Text("in your inventory and your saddlebag, if the items are stackable the");
            ImGui.Text("runner will try to \"clean up\" your inventory by moving the");
            ImGui.Text("items from the inventory to your saddlebag instead.");
            ImGui.Text("");
            ImGui.Text("It will only move duplicated items that are found in your saddlebag.");
        });
    }

    private static void DrawAutoSellWorthlessItemOptions()
    {
        bool sellWorthlessItems = AWC.Config.SellWorthlessItems;
        if (ImGui.Checkbox("Auto sell worthless items", ref sellWorthlessItems)) {
            AWC.Config.SellWorthlessItems = sellWorthlessItems;
        }

        ImGui.SameLine();

        if (ImGuiEx.IconButton(FontAwesomeIcon.Filter)) {
            AWC.Instance.ToggleItemFilterUi();
        }

        ImGuiEx.Tooltip("Opens the items filter options window");

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Will run all the items in your inventory through your selected filters,");
            ImGui.Text("and then sell any items that doesn't meet your filters.");

            ImGui.Text("Requires ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" and ");
            StatusText.Draw(VNavMeshIPC.IsEnabled, "VNavMesh");
            ImGui.Text(" to be enabled");
        });

        Disabled.Draw(!AWC.Config.SellWorthlessItems, () =>
        {
            ImGui.Text("Sell items every");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(80 * ImGuiHelpers.GlobalScale);

            uint sellWorthlessItemsRunInterval = AWC.Config.SellWorthlessItemsRunInterval;
            if (Range.Draw("runs###sell-worthless-items-run-interval", ref sellWorthlessItemsRunInterval, 1, 100)) {
                AWC.Config.SellWorthlessItemsRunInterval = sellWorthlessItemsRunInterval;
            }
        });
    }

    private static void DrawAutoSpendTomestoneOptions()
    {
        bool autoSpendUncappedTomestones = AWC.Config.SpendUncappedTomestones;
        if (ImGui.Checkbox("Auto Spend Uncapped Tomestones", ref autoSpendUncappedTomestones)) {
            AWC.Config.SpendUncappedTomestones = autoSpendUncappedTomestones;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Will teleport to Solution Nine or the Phantom Village and buy");
            ImGui.Text("your selected items with your uncapped tomestones, each time");
            ImGui.Text("an item is bought the amount is decremented from the counter");
            ImGui.Text("below, until it hits zero and it's removed.");

            ImGui.Text("Requires ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" and ");
            StatusText.Draw(VNavMeshIPC.IsEnabled, "VNavMesh");
            ImGui.Text(" to be enabled");
        });

        Disabled.Draw(!AWC.Config.SpendUncappedTomestones, () =>
        {
            ImGui.Text("Buy @");
            ImGui.SameLine();

            ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);

            uint autoBuyWithUncappedTomestones = AWC.Config.SpendUncappedTomestoneThreshold;
            if (Range.Draw("##BuyTomestones@", ref autoBuyWithUncappedTomestones, 1, 2000)) {
                AWC.Config.SpendUncappedTomestoneThreshold = autoBuyWithUncappedTomestones;
            }

            Card.DrawSubtle(
                title: $"Item{(AWC.Config.SpendUncappedTomestoneItems.Count != 1 ? "s" : "")} to buy",
                collapsible: false,
                bodyContent: DrawTomestoneItemsAndSelector
            );
        });
    }

    private static void DrawTomestoneItemsAndSelector()
    {
        double tomesNeeded = 0D;

        if (AWC.Config.SpendUncappedTomestoneItems.Count == 0) {
            ImGui.TextColored(Theme.TextMuted, "No items selected");
        } else {
            for (int index = 0; index < AWC.Config.SpendUncappedTomestoneItems.Count; index++) {
                if (index < 0 || index >= AWC.Config.SpendUncappedTomestoneItems.Count) {
                    continue;
                }

                TomestoneItem configItem = AWC.Config.SpendUncappedTomestoneItems[index];
                Enums.TomestoneItem? tomestoneItem = TomestoneItemHelper.GetTomestoneItemFromItemId(configItem.ItemId);
                bool isLastItem = index == AWC.Config.SpendUncappedTomestoneItems.Count - 1;

                DrawTomestoneConfigItem(
                    AWC.Config.SpendUncappedTomestoneItems,
                    configItem,
                    tomestoneItem,
                    isLastItem,
                    index
                );

                if (tomestoneItem != null && !isLastItem) {
                    tomesNeeded += tomestoneItem.Cost * configItem.Quantity;
                }
            }
        }

        Card.Separator();

        if (ImGui.BeginCombo("##PreferredUncappedTomestoneItem", "Add item...")) {
            foreach (Enums.TomestoneItem item in TomestoneItemHelper.GetTomestoneItems()) {
                if (InventoryHelper.TryGetSheetItemFromItemId(item.ItemId, out Item itemObj)) {
                    ItemIcon.Draw(itemObj.Icon);
                }

                if (ImGui.Selectable(item.Name)) {
                    AWC.Config.SpendUncappedTomestoneItems.Add(new TomestoneItem { ItemId = item.ItemId, Quantity = 1 });
                }
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();

        if (ImGui.Button("Add Relic Items")) {
            List<Enums.TomestoneItem> tomestoneItems = TomestoneItemHelper.GetTomestoneItems()
                .Where(item => item.NPC == TomestoneNPC.Relic)
                .ToList();

            int itemCount = Math.Min(
                tomestoneItems.Count,
                ImGuiEx.Ctrl
                    ? 1
                    : ImGuiEx.Shift
                        ? 2
                        : ImGuiEx.Alt
                            ? 3
                            : tomestoneItems.Count
            );

            foreach (Enums.TomestoneItem item in tomestoneItems.Skip(Math.Max(0, tomestoneItems.Count - itemCount))) {
                AWC.Config.SpendUncappedTomestoneItems.Add(new TomestoneItem { ItemId = item.ItemId, Quantity = 3 });
            }
        }

        ImGuiEx.Tooltip(
            "Adds all relic items, or hold down a modifier key to add a limited number of items.\n" +
            "CTRL    = Adds the last relic material\n" +
            "SHIFT   = Adds the last two relic materials\n" +
            "ALT        = Adds the last three relic materials"
        );

        if (tomesNeeded == 0) {
            return;
        }

        Card.Separator();

        double runsNeeded = Math.Max(Math.Ceiling(tomesNeeded / Constants.UncappedTomesPerRun), 1);

        ImGuiEx.TextCentered(Theme.TextMuted, $"Requires a total of {tomesNeeded:##,###} tomes, taking {runsNeeded:##,###} runs");
    }

    private static void DrawTomestoneConfigItem(
        List<TomestoneItem> items,
        TomestoneItem configItem,
        Enums.TomestoneItem? tomestoneItem,
        bool isLastItem,
        int index
    )
    {
        ImGui.PushID($"tomestone-item-{index}-{configItem.ItemId}");

        if (InventoryHelper.TryGetSheetItemFromItemId(configItem.ItemId, out Item itemObj)) {
            ItemIcon.Draw(itemObj.Icon);
        }

        ImGui.Text(tomestoneItem?.Name ?? $"Unknown item ({configItem.ItemId})");


        float quantityWidth = 100f * ImGuiHelpers.GlobalScale;
        float iconButtonWidth = ImGui.GetFrameHeight() * 4;
        float controlsWidth = quantityWidth + iconButtonWidth + (Spacing * 3f);

        float controlsX = Math.Max(ImGui.GetCursorPosX(), ImGui.GetWindowContentRegionMax().X - controlsWidth);
        ImGui.SameLine();
        ImGui.SetCursorPosX(controlsX);
        ImGui.SetNextItemWidth(quantityWidth);

        if (isLastItem) {
            string infiniteSymbol = "∞";
            Disabled.Draw(true, () => ImGui.InputText("##tomestone-item-quantity", ref infiniteSymbol, 2));
        } else {
            int quantity = (int)Math.Max(1, configItem.Quantity);
            if (ImGui.InputInt("##tomestone-item-quantity", ref quantity, 1, 10)) {
                configItem.Quantity = (uint)Math.Clamp(quantity, 1, 9999);
            }
        }

        ImGui.SameLine(0f, Spacing);
        Disabled.Draw(index == 0, () =>
        {
            if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowUp)) {
                (items[index], items[index - 1]) = (items[index - 1], items[index]);
            }
        });

        ImGui.SameLine(0f, Spacing);
        Disabled.Draw(isLastItem, () =>
        {
            if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowDown)) {
                (items[index], items[index + 1]) = (items[index + 1], items[index]);
            }
        });

        ImGui.SameLine(0f, Spacing);
        if (ImGuiEx.IconButton(FontAwesomeIcon.Trash) && ImGuiEx.Ctrl) {
            items.RemoveAt(index);
        }

        if (ImGui.IsItemHovered()) {
            ImGuiEx.Tooltip("Hold down CTRL + Click to delete this item");
        }

        ImGui.PopID();
    }
}
