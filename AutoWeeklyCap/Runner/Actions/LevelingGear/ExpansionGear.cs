using FFXIVClientStructs.FFXIV.Client.UI;

namespace AutoWeeklyCap.Runner.Actions.LevelingGear;

public abstract class ExpansionGear : QueueableAction
{
    private static readonly string[] RelatedAddonsToClose = ["SelectIconString", "SelectString", "Shop", "SelectYesno"];

    public abstract int MinimumLevel { get; }
    public abstract int ItemLevelThreshold { get; }

    protected abstract string TerritoryAetheriteName { get; }
    protected abstract uint TerritoryDataId { get; }

    public abstract void OpenVendorWindow(ItemSlot slot, ItemType type);

    protected abstract (Vector3, uint) GetVendorData(ItemSlot slot);

    public void MoveToTerritory()
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

            return Player.Territory.RowId == TerritoryDataId && PlayerHelper.IsReady;
        }, "waiting for player to be in territory");
    }

    public void MoveToVendor(ItemSlot slot)
    {
        Enqueue(() =>
        {
            var (location, _) = GetVendorData(slot);

            return MovementHelper.MoveTo(location);
        }, "start moving to npc location");
    }

    public void BuyShopUpgradeMatchingJob(ItemSlot slot, ItemType type, PlayerJob job)
    {
        Enqueue(() =>
        {
            unsafe {
                if (!AddonHelper.TryGetReadyAddon("Shop", out var addon)) {
                    return false;
                }

                var matchingShopItem = ShopHelper.GetMatchingShopItem((AddonShop*)addon, slot, type, job, MinimumLevel);
                if (matchingShopItem == null) {
                    AWC.Log.Debug($"{nameof(ExpansionGear)}: No match found for slot: {slot} | Type: {type} | Job: {job} | Required Level: {MinimumLevel}");
                    return false;
                }

                // TODO: Buy the found item
                AWC.Log.Debug($"{nameof(ExpansionGear)}: Found matching item: Name: {matchingShopItem.Name} | Index: {matchingShopItem.Index} | Type: {matchingShopItem.Type} | ItemId: {matchingShopItem.ItemId}");

                return true;
            }
        }, "buy shop item");
    }

    public void CloseShopWindows()
    {
        Enqueue(() => AddonHelper.CloseAddons(RelatedAddonsToClose), "close shop window");
    }
}
