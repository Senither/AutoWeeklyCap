using AutoWeeklyCap.Contracts.Runner;

namespace AutoWeeklyCap.Runner.Actions.LevelingGear;

public class Shadowbringers : ExpansionGear
{
    protected override string Name => nameof(Shadowbringers);

    public override int MinimumLevel => 70;
    protected override int ItemLevelThreshold => 385;

    protected override string TerritoryAetheriteName => "Kogane Dori Markets";
    protected override uint TerritoryDataId => 628;

    private readonly Vector3 _weaponVendorLocation = new(39.948685f, 4.000001f, 51.713814f);
    private readonly Vector3 _armorerVendorLocation = new(35.20646f, 4.000001f, 51.705814f);
    private readonly Vector3 _jewelerVendorLocation = new(29.879074f, 4.000001f, 51.7199f);

    private readonly uint _weaponVendorDataId = 1018990u;
    private readonly uint _armorerVendorDataId = 1018989u;
    private readonly uint _jewelerVendorDataId = 1018988u;

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
                    return ObjectHelper.OpenShopUsingSelectIconStringWindow(id, location, 4);
                }

                if (slot.IsGear()) {
                    return ObjectHelper.OpenShopUsingSelectIconStringWithSelectStringWindow(id, location, type.IsWarGear() ? 0 : 1, 4);
                }

                return ObjectHelper.OpenShopUsingSelectIconStringWindow(id, location, 0);
            } catch (Exception) {
                // ignored
            }

            return false;
        }, "opening leveling gear shop");
    }

    protected override (Vector3, uint) GetVendorData(ItemSlot slot)
    {
        if (slot.IsWeapon()) {
            return (_weaponVendorLocation, _weaponVendorDataId);
        }

        if (slot.IsGear()) {
            return (_armorerVendorLocation, _armorerVendorDataId);
        }

        return (_jewelerVendorLocation, _jewelerVendorDataId);
    }
}
