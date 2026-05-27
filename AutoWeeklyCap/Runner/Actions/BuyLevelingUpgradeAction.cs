using AutoWeeklyCap.Runner.Actions.LevelingGear;

namespace AutoWeeklyCap.Runner.Actions;

public class BuyLevelingUpgradeAction : BaseAction
{
    protected override string Name => nameof(BuyLevelingUpgradeAction);
    protected override string[] AddonsToClose { get; } = ["SelectIconString", "SelectString", "Shop", "SelectYesno"];

    private readonly List<ExpansionGear> _expansionGears =
    [
        new Heavensward(),
    ];

    protected override bool Run(params object[] args)
    {
        if (!LifestreamIPC.IsEnabled || !VNavMeshIPC.IsEnabled || !StylistIPC.IsEnabled) {
            return false;
        }

        var job = PlayerHelper.GetCurrentJob();
        var level = PlayerHelper.GetJobLevel(job);

        if (Constants.CurrentMaxLevel == level) {
            return false;
        }

        // 1. Get an instance of the gear container that best matches the current level
        var gearExpansion = GetExpansionGear(level);
        if (gearExpansion == null) {
            return false;
        }

        // 2. Check if the current item level is below the threshold for that gear container
        var itemLevel = InventoryHelper.GetCurrentItemLevel();
        if (gearExpansion.ItemLevelThreshold <= itemLevel) {
            // 3.a If the item level is at or above the threshold, stop the action
            return false;
        }

        // 3.b If the item level is below the threshold, call a method to navigate to the vendor territory
        gearExpansion.MoveToTerritory();

        // 4. Get the item and slot that needs to be upgraded, and move to the vendor location
        var (item, slot) = InventoryHelper.GetLowestEquippedItemLevelItem();
        gearExpansion.MoveToVendor(slot);

        // 4.a Get the item type for the current job and slot, and open the correct shop window
        var type = ItemTypeExtensions.GetItemTypeFromJobAndSlot(job, slot);
        gearExpansion.OpenVendorWindow(slot, type);

        // 4.b The call to buy the item should then be made, based off the slot and item type
        gearExpansion.BuyShopUpgradeMatchingJob(slot, type, job);


        // 4.c After the item is bought, close all the addons, call Stylist to equip the gear upgrade
        // 4.d Wait for Stylist, when it's done, loop back to step 2

        AWC.Log.Debug($"Job: {job} | Item: {item?.Name ?? "<empty>"} | Item Level: {item?.LevelItem.RowId ?? 0} | Slot: {slot} | Type: {type}");

        return false;
    }

    private ExpansionGear? GetExpansionGear(int level)
    {
        return _expansionGears.FirstOrDefault(expansionGear => expansionGear.MinimumLevel <= level);
    }
}
