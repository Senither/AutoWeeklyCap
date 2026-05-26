namespace AutoWeeklyCap.Runner.Actions.LevelingGear;

public abstract class ExpansionGear : QueueableAction
{
    public abstract int MinimumLevel { get; }
    public abstract int ItemLevelThreshold { get; }

    protected abstract string TerritoryAetheriteName { get; }
    protected abstract uint TerritoryDataId { get; }

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

    public abstract void MoveToVendor(ItemSlot slot);

    public abstract void OpenVendorWindow(ItemSlot slot, ItemType type);

    protected static unsafe bool OpenSelectIconStringWindow(Vector3 location, uint dataId, int iconStringIndex)
    {
        var vendor = ObjectHelper.FindGameObject(dataId, location);
        if (vendor == null) {
            return false;
        }

        if (AddonHelper.TryGetReadyAddon("Shop", out _)) {
            return true;
        }

        if (AddonHelper.TryGetReadyAddon("SelectIconString", out _)) {
            AddonHelper.ClickSelectIconString(iconStringIndex);
        } else {
            ObjectHelper.InteractWithObject(vendor);
        }

        return false;
    }

    protected static unsafe bool OpenSelectIconStringWithSelectStringWindow(Vector3 location, uint dataId, int iconStringIndex, int selectStringIndex)
    {
        var vendor = ObjectHelper.FindGameObject(dataId, location);
        if (vendor == null) {
            return false;
        }

        if (AddonHelper.TryGetReadyAddon("Shop", out _)) {
            return true;
        }

        if (AddonHelper.TryGetReadyAddon("SelectString", out _)) {
            AddonHelper.ClickSelectString(selectStringIndex);
        } else if (AddonHelper.TryGetReadyAddon("SelectIconString", out _)) {
            AddonHelper.ClickSelectIconString(iconStringIndex);
        } else {
            ObjectHelper.InteractWithObject(vendor);
        }

        return false;
    }
}
