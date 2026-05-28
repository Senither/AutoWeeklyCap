namespace AutoWeeklyCap.Runner.Actions.LevelingGear;

public class Stormblood : Heavensward
{
    protected override string Name => nameof(Stormblood);

    public override int MinimumLevel => 60;
    public override int ItemLevelThreshold => 255;

    protected override void OpenVendorWindow(ItemSlot slot, ItemType type)
    {
        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("BuyingLevelingGearViaNPC", 250)) {
                return false;
            }

            var (location, id) = GetVendorData(slot);

            try {
                return slot.IsAccessory()
                    ? ObjectHelper.OpenShopUsingSelectIconStringWindow(id, location, 0)
                    : ObjectHelper.OpenShopUsingSelectIconStringWithSelectStringWindow(id, location, type.IsWarGear() ? 0 : 1, 7);
            } catch (Exception) {
                // ignored
            }

            return false;
        }, "opening leveling gear shop");
    }
}
