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
        public string GetName()
        {
            return type switch
            {
                ItemPriceType.Minimum => "Minimum price",
                ItemPriceType.Recent => "Recent price",
                ItemPriceType.Average => "Average price",
                ItemPriceType.Maximum => "Maximum between all options",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public uint GetPrice(MarketBoardItemPrice? item)
        {
            if (item == null) {
                return 0u;
            }

            return type switch
            {
                ItemPriceType.Minimum => ItemPriceType.LoadPriceFromPriceList([item.MinListing, item.AverageListing, item.RecentListing]),
                ItemPriceType.Recent => ItemPriceType.LoadPriceFromPriceList([item.RecentListing, item.AverageListing, item.MinListing]),
                ItemPriceType.Average => ItemPriceType.LoadPriceFromPriceList([item.AverageListing, item.MinListing, item.RecentListing]),
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

        private static uint LoadPriceFromPriceList(MarketBoardItemPriceList?[] items)
        {
            foreach (var item in items) {
                if (item == null) {
                    continue;
                }

                if (item.WorldPrice > 0) {
                    return item.WorldPrice;
                }

                if (item.DcPrice > 0) {
                    return item.DcPrice;
                }

                if (item.RegionPrice > 0) {
                    return item.RegionPrice;
                }
            }

            return 0u;
        }
    }
}
