using FFXIVClientStructs.FFXIV.Client.UI;

namespace AutoWeeklyCap.Runner.Actions.LevelingGear;

public abstract class ExpansionGear : QueueableAction
{
    private static readonly string[] RelatedAddonsToClose = ["SelectIconString", "SelectString", "Shop", "SelectYesno"];

    public abstract int MinimumLevel { get; }
    protected abstract int ItemLevelThreshold { get; }

    protected abstract string TerritoryAetheriteName { get; }
    protected abstract uint TerritoryDataId { get; }

    public bool IsAboveItemLevelThreshold(int itemLevel)
    {
        var levelingProfile = AWC.Config.LevelJobs.PreferredGearingProfile;

        return ItemLevelThreshold - levelingProfile.GetSubtractedItemLevel() <= itemLevel;
    }

    public void EnqueueSequence(PlayerJob job)
    {
        var (_, slot) = InventoryHelper.GetLowestEquippedItemLevelItem();
        MoveToTerritory();
        MoveToVendor(slot);

        var type = ItemTypeExtensions.GetItemTypeFromJobAndSlot(job, slot);
        OpenVendorWindow(slot, type);
        BuyShopUpgradeMatchingJob(slot, type, job);
        CloseShopWindows();

        AWC.Log.Debug($"{nameof(ExpansionGear)}: Starting sequence with data (Job={job}, Slot={slot}, Type={type}, MinimumLevel={MinimumLevel}, ItemLevelThreshold={ItemLevelThreshold})");
    }

    protected abstract void OpenVendorWindow(ItemSlot slot, ItemType type);

    protected abstract (Vector3, uint) GetVendorData(ItemSlot slot);

    private void MoveToTerritory()
    {
        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("NavigatingToGearTerritory", 500)) {
                return false;
            }

            if (Player.Territory.RowId == TerritoryDataId) {
                return true;
            }

            if (LifestreamIPC.IsBusy()) {
                return false;
            }

            LifestreamIPC.ExecuteCommand(TerritoryAetheriteName);

            return true;
        }, "start moving to territory");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("NavigatingToGearTerritory", 500)) {
                return false;
            }

            return Player.Territory.RowId == TerritoryDataId && PlayerHelper.IsReady && !LifestreamIPC.IsBusy();
        }, "waiting for player to be in territory");
    }

    private void MoveToVendor(ItemSlot slot)
    {
        Enqueue(() =>
        {
            var (location, _) = GetVendorData(slot);

            return MovementHelper.MoveTo(location);
        }, "start moving to npc location");
    }

    private void BuyShopUpgradeMatchingJob(ItemSlot slot, ItemType type, PlayerJob job)
    {
        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("BuyShopUpgradeMatchingJob", 500)) {
                return false;
            }

            unsafe {
                if (AddonHelper.TryGetReadyAddon("SelectYesno", out _)) {
                    AddonHelper.ClickSelectYesno();
                    return true;
                }

                if (!AddonHelper.TryGetReadyAddon("Shop", out var addon)) {
                    return false;
                }

                var matchingShopItem = ShopHelper.GetMatchingShopItem((AddonShop*)addon, slot, type, job, MinimumLevel);
                if (matchingShopItem == null) {
                    AWC.Log.Debug($"{nameof(ExpansionGear)}: No match found for slot: {slot} | Type: {type} | Job: {job} | Required Level: {MinimumLevel}");
                    return false;
                }

                AWC.Log.Debug($"{nameof(ExpansionGear)}: Found matching item: Name: {matchingShopItem.Name} | Index: {matchingShopItem.Index} | Type: {matchingShopItem.Type} | ItemId: {matchingShopItem.ItemId}");
                AddonHelper.ClickShopItem(matchingShopItem.Index);

                return false;
            }
        }, "buy shop item");

        EnqueueDelay(500);
    }

    private void CloseShopWindows()
    {
        Enqueue(() => AddonHelper.CloseAddons(RelatedAddonsToClose), "close shop window");
    }
}
