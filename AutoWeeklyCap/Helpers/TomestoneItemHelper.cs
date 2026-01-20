using System;
using System.Collections.Generic;
using ECommons;

namespace AutoWeeklyCap.Helpers;

public record TomestoneItem(int Index, int Cost, string Name)
{
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
        Items.Add(new TomestoneItem(0, 20, "Turali Pigment"));
        Items.Add(new TomestoneItem(1, 20, "Mastodon Pelt"));
        Items.Add(new TomestoneItem(2, 20, "Everkeep Resin"));
        Items.Add(new TomestoneItem(3, 20, "Insulating Varnish"));
        Items.Add(new TomestoneItem(4, 20, "Double Duracoat"));
        Items.Add(new TomestoneItem(5, 20, "Yollal Extract"));

        // Previous path items (Costs 10)
        Items.Add(new TomestoneItem(6, 10, "Neo Abrasive"));
        Items.Add(new TomestoneItem(7, 10, "Diatryma Pelt"));
        Items.Add(new TomestoneItem(8, 10, "Cronopio Skin"));
        Items.Add(new TomestoneItem(9, 10, "Hydrophobic Preservative"));
        Items.Add(new TomestoneItem(10, 10, "Dichromatic Compound"));
        Items.Add(new TomestoneItem(11, 10, "Shaaloani Coke"));
        Items.Add(new TomestoneItem(12, 10, "Potsworn's Abrasive"));
        Items.Add(new TomestoneItem(13, 10, "Pelupelu Yarn"));
        Items.Add(new TomestoneItem(14, 10, "Purussaurus Skin"));
        Items.Add(new TomestoneItem(15, 10, "Xbr'aal Varnish"));
        Items.Add(new TomestoneItem(16, 10, "Airbright Coolant"));
        Items.Add(new TomestoneItem(17, 10, "Glossy Dried Ether"));
    }

    public static LinkedList<TomestoneItem> GetTomestoneItems()
    {
        return Items;
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
