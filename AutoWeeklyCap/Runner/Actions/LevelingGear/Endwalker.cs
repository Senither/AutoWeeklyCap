namespace AutoWeeklyCap.Runner.Actions.LevelingGear;

public class Endwalker : ExpansionGear
{
    protected override string Name => nameof(Endwalker);

    public override int MinimumLevel => 80;
    protected override int ItemLevelThreshold => 515;

    protected override string TerritoryAetheriteName => "Old Sharlayan";
    protected override uint TerritoryDataId => 962;

    private readonly Vector3 _vendorLocation = new(43.60633f, 5.1499996f, -74.91691f);
    private readonly uint _vendorDataId = 1037049u;

    protected override void OpenVendorWindow(ItemSlot slot, ItemType type)
    {
        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("BuyingLevelingGearViaNPC", 250)) {
                return false;
            }

            var (location, id) = GetVendorData(slot);

            try {
                if (slot.IsWeapon()) {
                    return ObjectHelper.OpenShopUsingSelectIconStringWindow(id, location, 0);
                }

                if (slot.IsGear()) {
                    return ObjectHelper.OpenShopUsingSelectIconStringWindow(id, location, type.IsWarGear() ? 1 : 2);
                }

                return ObjectHelper.OpenShopUsingSelectIconStringWindow(id, location, 5);
            } catch (Exception) {
                // ignored
            }

            return false;
        }, "opening leveling gear shop");
    }

    protected override (Vector3, uint) GetVendorData(ItemSlot slot)
    {
        return (_vendorLocation, _vendorDataId);
    }
}
