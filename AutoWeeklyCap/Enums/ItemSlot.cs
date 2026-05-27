using Lumina.Excel.Sheets;

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
    public static ItemSlot? FromItem(Item item)
    {
        return item.EquipSlotCategory.RowId switch
        {
            1 => ItemSlot.MainHand,
            2 => ItemSlot.OffHand,
            3 => ItemSlot.Head,
            4 => ItemSlot.Body,
            5 => ItemSlot.Hands,
            7 => ItemSlot.Legs,
            8 => ItemSlot.Feet,
            9 => ItemSlot.Earring,
            10 => ItemSlot.Neckless,
            11 => ItemSlot.Wrists,
            12 => ItemSlot.RightRing,
            _ => null
        };
    }

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

        public bool IsMatch(ItemSlot expected)
        {
            if (expected == slot) {
                return true;
            }

            return expected switch
            {
                ItemSlot.LeftRing when slot == ItemSlot.RightRing => true,
                ItemSlot.RightRing when slot == ItemSlot.LeftRing => true,
                _ => false
            };
        }
    }
}
