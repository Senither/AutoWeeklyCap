namespace AutoWeeklyCap.Config;

[Serializable]
public class ItemFilterOptions
{
    public ItemPriceType ItemPriceType = ItemPriceType.Average;

    public uint GilThreshold { get; set; } = 1000;
    public uint ItemLevelThreshold { get; set; } = Constants.CurrentMaxItemLevel - 145;
    public bool ExcludeMateria { get; set; } = false;
    public bool ExcludeFood { get; set; } = false;
    public bool ExcludePotions { get; set; } = false;
    public bool ExcludeDyes { get; set; } = false;

    // ReSharper disable once UnusedMember.Global
    [Obsolete("use AlwaysKeepItems instead")]
    public HashSet<uint> BlacklistedItems {
        set => AlwaysKeepItems = value;
    }

    public HashSet<uint> AlwaysKeepItems { get; set; } = [];
    public HashSet<uint> AlwaysSellItems { get; set; } = [];
}
