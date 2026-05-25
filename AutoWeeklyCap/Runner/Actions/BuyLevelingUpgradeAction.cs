namespace AutoWeeklyCap.Runner.Actions;

public class BuyLevelingUpgradeAction : BaseAction
{
    protected override string Name => nameof(BuyLevelingUpgradeAction);
    protected override string[] AddonsToClose { get; } = ["SelectIconString", "SelectString", "Shop", "SelectYesno"];

    protected override bool Run(params object[] args)
    {
        if (!LifestreamIPC.IsEnabled || !VNavMeshIPC.IsEnabled || !StylistIPC.IsEnabled) {
            return false;
        }

        var job = PlayerHelper.GetCurrentJob();
        var level = PlayerHelper.GetJobLevel(job);

        // 1. Get an instance of the gear container that best matches the current level
        // 2. Check if the current item level is below the threshold for that gear container
        // 3.a If the item level is at or above the threshold, stop the action
        // 3.b If the item level is below the threshold, call a method to navigate to the vendors
        // 4. Get the item and slot that needs to be upgraded, and give it to the gear container
        // 4.a The gear container should redirect this to the right vendor, walk there, and open the correct shop window
        // 4.b The call to buy the item should then be made, based off the slot and item type
        // 4.c After the item is bought, close all the addons, call Stylist to equip the gear upgrade
        // 4.d Wait for Stylist, when it's done, loop back to step 2

        var (item, slot) = InventoryHelper.GetLowestEquippedItemLevelItem();
        var type = ItemTypeExtensions.GetItemTypeFromJobAndSlot(job, slot);

        AWC.Log.Debug($"Job: {job} | Item: {item?.Name ?? "<empty>"} | Item Level: {item?.LevelItem.RowId ?? 0} | Slot: {slot} | Type: {type}");

        return true;
    }
}
