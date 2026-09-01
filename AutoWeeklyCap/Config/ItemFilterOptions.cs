namespace AutoWeeklyCap.Config;

[Serializable]
public class ItemFilterOptions
{
    public ItemPriceType ItemPriceType = ItemPriceType.Average;

    public uint GilThreshold { get; set; } = 1000;
    public uint ItemLevelThreshold { get; set; } = 0;
    public bool ExcludeMateria { get; set; } = false;
    public bool ExcludeFood { get; set; } = false;
    public bool ExcludePotions { get; set; } = false;
    public bool ExcludeDyes { get; set; } = false;

    public HashSet<uint> BlacklistedItems { get; set; } = [];
}
