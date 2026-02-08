// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.Helpers;

public enum TomestoneNPC
{
    Material = 0,
    Relic = 1
}

public record TomestoneItem(TomestoneNPC NPC, int Index, int Cost, string Name)
{
    public readonly TomestoneNPC NPC = NPC;
    public readonly int Index = Index;
    public readonly int Cost = Cost;
    public readonly string Name = Name;

    public int CalculateQuantityForGivenTomestones(int tomestones)
    {
        // ReSharper disable once PossibleLossOfFraction
        return (int)Math.Floor((double)(tomestones / Cost));
    }
}

public static class TomestoneItemHelper
{
    private static readonly LinkedList<TomestoneItem> Items = [];

    public static void RegisterTomestoneItems()
    {
        Items.Clear();

        // Current items (Costs 20)
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 0, 20, "Turali Pigment"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 1, 20, "Mastodon Pelt"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 2, 20, "Everkeep Resin"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 3, 20, "Insulating Varnish"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 4, 20, "Double Duracoat"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 5, 20, "Yollal Extract"));

        // Previous path items (Costs 10)
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 6, 10, "Neo Abrasive"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 7, 10, "Diatryma Pelt"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 8, 10, "Cronopio Skin"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 9, 10, "Hydrophobic Preservative"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 10, 10, "Dichromatic Compound"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 11, 10, "Shaaloani Coke"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 12, 10, "Potsworn's Abrasive"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 13, 10, "Pelupelu Yarn"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 14, 10, "Purussaurus Skin"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 15, 10, "Xbr'aal Varnish"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 16, 10, "Airbright Coolant"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 17, 10, "Glossy Dried Ether"));

        // Relic items (Cost 500)
        Items.Add(new TomestoneItem(TomestoneNPC.Relic, 0, 500, "Arcanite (Relic)"));
        Items.Add(new TomestoneItem(TomestoneNPC.Relic, 1, 500, "Waxing Arcanite (Relic)"));
        Items.Add(new TomestoneItem(TomestoneNPC.Relic, 2, 500, "Waning Arcanite (Relic)"));
    }

    public static LinkedList<TomestoneItem> GetTomestoneItems()
    {
        return Items;
    }

    public static TomestoneItem? GetTomestoneItemFromNames(string? first, string? second)
    {
        var item = GetTomestoneItemFromName(first);
        if (item != null)
            return item;

        return GetTomestoneItemFromName(second);
    }

    public static TomestoneItem? GetTomestoneItemFromName(string? name)
    {
        if (name == null)
            return null;

        foreach (var item in Items)
        {
            if (item.Name == name)
                return item;
        }

        return null;
    }
}
