using AutoWeeklyCap.Contracts.Runner;

using ECommons.Configuration;

using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoWeeklyCap.Runner.Actions;

public class ExtractAction : BaseAction
{
    protected override string Name => nameof(ExtractAction);
    protected override string[] AddonsToClose { get; } = ["MaterializeDialog", "Materialize", "SelectYesno", "SelectString"];

    protected override bool Run(params object[] args)
    {
        if (!QuestManager.IsQuestComplete(66174)) {
            LogInfo("Stopping materia extraction, reason: player has not completed quest 66174 (Forging the Spirit)");
            return false;
        }

        List<SpiritbondedItem> items = GetSpiritboundedItemsList();
        if (items.Count == 0) {
            return false;
        }

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.ElementEarth, "Extracting materia");

        int currentSlot = -1;

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("Extract", 250)) {
                return false;
            }

            if (PlayerHelper.IsOccupied) {
                return false;
            }

            if (items.Count == 0) {
                return true;
            }

            if (InventoryHelper.GetEmptySlotsInBag() < 1) {
                LogInfo($"Stopping {Name}, reason: no items slot left");
                return true;
            }

            unsafe {
                if (!GenericHelpers.TryGetAddonByName("Materialize", out AtkUnitBase* addonMaterialize)) {
                    ActionManager.Instance()->UseAction(ActionType.GeneralAction, 14);
                    return false;
                }

                if (!GenericHelpers.IsAddonReady(addonMaterialize)) {
                    return false;
                }

                if (items[0].Slot != currentSlot) {
                    currentSlot = items[0].Slot;
                    AddonHelper.FireCallBack(addonMaterialize, false, 1, currentSlot);
                } else {
                    items.RemoveAt(0);
                    AddonHelper.FireCallBack(addonMaterialize, true, 2, 0);
                    AWC.Config.GetCurrentCharacterMetrics()?.IncrementMateriaCounter();
                }
            }

            return false;
        }, "extracting materia", 180_000); // 3 minutes

        Enqueue(() => AddonHelper.CloseAddons(AddonsToClose), "closing window");

        Enqueue(() =>
        {
            EzConfig.Save();
            return true;
        }, "saving metrics");

        return true;
    }

    private static List<SpiritbondedItem> GetSpiritboundedItemsList()
    {
        List<SpiritbondedItem> items = GetSpiritboundedItemsFromInventorySlots(0, [InventoryType.EquippedItems]);

        if (!AWC.Config.ExtractAll) {
            return items;
        }

        items.AddRange(GetSpiritboundedItemsFromInventorySlots(1, [InventoryType.ArmoryOffHand, InventoryType.ArmoryMainHand]));
        items.AddRange(GetSpiritboundedItemsFromInventorySlots(2, [InventoryType.ArmoryHead, InventoryType.ArmoryBody, InventoryType.ArmoryHands]));
        items.AddRange(GetSpiritboundedItemsFromInventorySlots(3, [InventoryType.ArmoryLegs, InventoryType.ArmoryFeets]));
        items.AddRange(GetSpiritboundedItemsFromInventorySlots(4, [InventoryType.ArmoryEar, InventoryType.ArmoryNeck]));
        items.AddRange(GetSpiritboundedItemsFromInventorySlots(5, [InventoryType.ArmoryWrist, InventoryType.ArmoryRings]));

        return items;
    }

    private static unsafe List<SpiritbondedItem> GetSpiritboundedItemsFromInventorySlots(int slot, InventoryType[] inventoryTypes)
    {
        List<SpiritbondedItem> items = [];

        foreach (var type in inventoryTypes) {
            InventoryContainer* inventory = InventoryManager.Instance()->GetInventoryContainer(type);

            for (var i = 0; i < inventory->Size; i++) {
                InventoryItem* item = inventory->GetInventorySlot(i);

                if (item->SpiritbondOrCollectability == 10_000) {
                    items.Add(new SpiritbondedItem { Slot = slot, Item = *item });
                }
            }
        }

        return items;
    }

    private record SpiritbondedItem
    {
        public int Slot { get; init; }
        public InventoryItem Item { get; init; }
    }
}
