// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.Enums;

public enum TomestoneNPC
{
    Material = 0,
    Relic = 1
}

public record TomestoneItem(TomestoneNPC NPC, uint ItemId, int Cost, string Name)
{
    public readonly TomestoneNPC NPC = NPC;
    public readonly uint ItemId = ItemId;
    public readonly int Cost = Cost;
    public readonly string Name = Name;

    public int CalculateQuantityForGivenTomestones(int tomestones)
    {
        // ReSharper disable once PossibleLossOfFraction
        return (int)Math.Floor((double)(tomestones / Cost));
    }
}
