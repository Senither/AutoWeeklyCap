namespace AutoWeeklyCap.Enums;

public enum ItemType
{
    Fending,
    Healing,
    Striking,
    Maiming,
    Slaying,
    Scouting,
    Aiming,
    Casting,

    Unknown
}

public static class ItemTypeExtensions
{
    public static ItemType GetItemTypeFromJobAndSlot(PlayerJob playerJob, ItemSlot slot)
    {
        return playerJob switch
        {
            PlayerJob.PLD or PlayerJob.WAR or PlayerJob.DRK or PlayerJob.GNB => ItemType.Fending,
            PlayerJob.WHM or PlayerJob.SCH or PlayerJob.AST or PlayerJob.SGE => ItemType.Healing,
            PlayerJob.BLM or PlayerJob.SMN or PlayerJob.RDM or PlayerJob.PCT => ItemType.Casting,
            PlayerJob.BRD or PlayerJob.MCH or PlayerJob.DNC => ItemType.Aiming,
            PlayerJob.MNK or PlayerJob.SAM => slot.IsGear() ? ItemType.Striking : ItemType.Slaying,
            PlayerJob.DRG or PlayerJob.RPR => slot.IsGear() ? ItemType.Maiming : ItemType.Slaying,
            PlayerJob.NIN or PlayerJob.VPR => slot.IsGear() ? ItemType.Scouting : ItemType.Aiming,

            _ => ItemType.Unknown
        };
    }

    extension(ItemType itemType)
    {
        public bool IsWarGear()
        {
            return itemType switch
            {
                ItemType.Fending => true,
                ItemType.Maiming => true,
                ItemType.Striking => true,
                ItemType.Scouting => true,
                ItemType.Aiming => true,
                ItemType.Slaying => true,


                ItemType.Healing => false,
                ItemType.Casting => false,
                ItemType.Unknown => false,

                _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
            };
        }

        public bool IsMagicGear()
        {
            return !itemType.IsWarGear();
        }
    }
}
