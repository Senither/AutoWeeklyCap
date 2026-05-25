namespace AutoWeeklyCap.Enums;

public enum ItemSlot
{
    MainHand,
    OffHand,

    Head,
    Body,
    Hands,
    Legs,
    Feet,

    Earring,
    Neckless,
    Wrists,
    RightRing,
    LeftRing,
}

public static class ItemSlotExtensions
{
    public static uint GetSlot(this ItemSlot slot)
    {
        return slot switch
        {
            ItemSlot.MainHand => 0,
            ItemSlot.OffHand => 1,

            ItemSlot.Head => 2,
            ItemSlot.Body => 3,
            ItemSlot.Hands => 4,
            ItemSlot.Legs => 6,
            ItemSlot.Feet => 7,

            ItemSlot.Earring => 8,
            ItemSlot.Neckless => 9,
            ItemSlot.Wrists => 10,
            ItemSlot.RightRing => 11,
            ItemSlot.LeftRing => 12,
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
        };
    }
}
