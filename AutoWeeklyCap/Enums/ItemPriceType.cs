using AutoWeeklyCap.Helpers.MarketBoard;

namespace AutoWeeklyCap.Enums;

public enum ItemPriceType
{
    Minimum,
    Recent,
    Average,
    Maximum
}

public static class ItemPriceTypeExtensions
{
    extension(ItemPriceType type)
    {
        public uint GetPrice(MarketBoardItemPrice? item)
        {
            if (item == null) {
                return 0u;
            }

            return type switch
            {
                ItemPriceType.Minimum => Math.Max(item.MinListing.DcPrice, item.MinListing.WorldPrice),
                ItemPriceType.Recent => Math.Max(item.RecentListing.DcPrice, item.RecentListing.WorldPrice),
                ItemPriceType.Average => Math.Max(item.AverageListing.DcPrice, item.AverageListing.WorldPrice),
                ItemPriceType.Maximum => Math.Max(
                    Math.Max(
                        ItemPriceType.Minimum.GetPrice(item),
                        ItemPriceType.Recent.GetPrice(item)
                    ),
                    ItemPriceType.Average.GetPrice(item)
                ),
                _ => 0u
            };
        }
    }
}
