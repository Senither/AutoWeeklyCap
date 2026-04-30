// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace AutoWeeklyCap.Helpers;

public static class TomestoneItemHelper
{
    private const int offset = 11;
    private static readonly LinkedList<TomestoneItem> Items = [];

    public static void RegisterTomestoneItems()
    {
        Items.Clear();

        // Current items (Costs 20)
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 0, 20, "Turali Pigment"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 1, 20, "Mastodon Pelt"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 2, 20, "Everkeep Resin"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 3, 20, "Insulating Varnish"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 4, 20, "Double Duracoat"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 5, 20, "Yollal Extract"));

        // Previous patch items (Costs 10)
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 6, 10, "Neo Abrasive"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 7, 10, "Diatryma Pelt"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 8, 10, "Cronopio Skin"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 9, 10, "Hydrophobic Preservative"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 10, 10, "Dichromatic Compound"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 11, 10, "Shaaloani Coke"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 12, 10, "Potsworn's Abrasive"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 13, 10, "Pelupelu Yarn"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 14, 10, "Purussaurus Skin"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 15, 10, "Xbr'aal Varnish"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 16, 10, "Airbright Coolant"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, offset + 17, 10, "Glossy Dried Ether"));

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
        return GetTomestoneItemFromName(first) ?? GetTomestoneItemFromName(second);
    }

    public static TomestoneItem? GetTomestoneItemFromName(string? name)
    {
        if (name == null) {
            return null;
        }

        return Items.FirstOrDefault(item => item.Name == name);
    }
}
