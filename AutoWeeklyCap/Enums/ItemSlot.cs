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
    extension(ItemSlot slot)
    {
        public uint GetSlot()
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

        public bool IsWeapon()
        {
            return slot switch
            {
                ItemSlot.MainHand or ItemSlot.OffHand => true,
                ItemSlot.Head or ItemSlot.Body or ItemSlot.Hands or ItemSlot.Legs or ItemSlot.Feet => false,
                ItemSlot.Earring or ItemSlot.Neckless or ItemSlot.Wrists or ItemSlot.RightRing or ItemSlot.LeftRing => false,

                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
            };
        }

        public bool IsGear()
        {
            return slot switch
            {
                ItemSlot.MainHand or ItemSlot.OffHand => false,
                ItemSlot.Head or ItemSlot.Body or ItemSlot.Hands or ItemSlot.Legs or ItemSlot.Feet => true,
                ItemSlot.Earring or ItemSlot.Neckless or ItemSlot.Wrists or ItemSlot.RightRing or ItemSlot.LeftRing => false,

                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
            };
        }

        public bool IsAccessory()
        {
            return slot switch
            {
                ItemSlot.MainHand or ItemSlot.OffHand => false,
                ItemSlot.Head or ItemSlot.Body or ItemSlot.Hands or ItemSlot.Legs or ItemSlot.Feet => false,
                ItemSlot.Earring or ItemSlot.Neckless or ItemSlot.Wrists or ItemSlot.RightRing or ItemSlot.LeftRing => true,

                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
            };
        }
    }
}
