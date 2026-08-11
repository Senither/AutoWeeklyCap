// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace AutoWeeklyCap.Helpers;

public static class TomestoneItemHelper
{
    private static readonly LinkedList<TomestoneItem> Items = [];

    public static void RegisterTomestoneItems()
    {
        Items.Clear();

        // Current items (Costs 20)
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 49226, 20, "Mastodon Pelt"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 49227, 20, "Turali Pigment"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 49225, 20, "Everkeep Resin"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 49223, 20, "Insulating Varnish"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 49224, 20, "Double Duracoat"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 49228, 20, "Yollal Extract"));

        // Previous patch items (Costs 10)
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 45986, 10, "Neo Abrasive"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 45988, 10, "Diatryma Pelt"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 45987, 10, "Cronopio Skin"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 45984, 10, "Hydrophobic Preservative"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 45989, 10, "Dichromatic Compound"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 45985, 10, "Shaaloani Coke"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 44143, 10, "Potsworn's Abrasive"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 44144, 10, "Pelupelu Yarn"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 44145, 10, "Purussaurus Skin"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 44141, 10, "Xbr'aal Varnish"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 44142, 10, "Airbright Coolant"));
        Items.Add(new TomestoneItem(TomestoneNPC.Material, 44146, 10, "Glossy Dried Ether"));

        // Relic items (Cost 500)
        Items.Add(new TomestoneItem(TomestoneNPC.Relic, 47750, 500, "Arcanite (Relic)"));
        Items.Add(new TomestoneItem(TomestoneNPC.Relic, 46850, 500, "Waxing Arcanite (Relic)"));
        Items.Add(new TomestoneItem(TomestoneNPC.Relic, 50058, 500, "Waning Arcanite (Relic)"));
        Items.Add(new TomestoneItem(TomestoneNPC.Relic, 50977, 500, "Ecliptic Arcanite (Relic)"));
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

    public static TomestoneItem? GetTomestoneItemFromItemId(uint itemId)
    {
        return Items.FirstOrDefault(item => item.ItemId == itemId);
    }
}
