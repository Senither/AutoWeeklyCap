using AutoWeeklyCap.Contracts.Runner;

namespace AutoWeeklyCap.Runner.Actions.LevelingGear;

public class Dawntrail : ExpansionGear
{
    protected override string Name => nameof(Dawntrail);

    public override int MinimumLevel => 90;
    protected override int ItemLevelThreshold => 645;

    protected override string TerritoryAetheriteName => "Tuliyollal";
    protected override uint TerritoryDataId => 1185;

    private readonly Vector3 _vendorLocation = new(-31.276203f, -10.0000105f, 81.415146f);
    private readonly uint _vendorDataId = 1048377u;

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
