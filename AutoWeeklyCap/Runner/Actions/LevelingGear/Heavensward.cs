namespace AutoWeeklyCap.Runner.Actions.LevelingGear;

public class Heavensward : ExpansionGear
{
    protected override string Name => nameof(Heavensward);

    public override int MinimumLevel => 50;
    public override int ItemLevelThreshold => 115;

    protected override string TerritoryAetheriteName => "The Jeweled Crozier";
    protected override uint TerritoryDataId => 419;

    private readonly Vector3 _weaponVendorLocation = new(-215.78963f, -16.034918f, -60.805546f);
    private readonly Vector3 _armorerVendorLocation = new(-204.95792f, -16.034918f, -51.679325f);
    private readonly Vector3 _jewelerVendorLocation = new(-188.94354f, -12.634914f, -40.540092f);

    private readonly uint _weaponVendorDataId = 1011203u;
    private readonly uint _armorerVendorDataId = 1011204u;
    private readonly uint _jewelerVendorDataId = 1011200u;

    public override void MoveToVendor(ItemSlot slot)
    {
        Enqueue(() =>
        {
            var (location, _) = GetVendorData(slot);

            return MovementHelper.MoveTo(location);
        }, "start moving to npc location");
    }

    public override void OpenVendorWindow(ItemSlot slot, ItemType type)
    {
        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("BuyingLevelingGearViaNPC", 250)) {
                return false;
            }

            var (location, id) = GetVendorData(slot);

            try {
                return slot.IsAccessory()
                    ? OpenSelectIconStringWindow(location, id, 0)
                    : OpenSelectIconStringWithSelectStringWindow(location, id, type.IsWarGear() ? 0 : 1, 2);
            } catch (Exception) {
                // ignored
            }

            return false;
        }, "opening leveling gear shop");
    }

    private (Vector3, uint) GetVendorData(ItemSlot slot)
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
