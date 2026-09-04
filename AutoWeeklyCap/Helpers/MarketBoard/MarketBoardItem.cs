namespace AutoWeeklyCap.Helpers.MarketBoard;

public class MarketBoardItem
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(3);

    public required bool IsLoaded { get; init; }

    public required uint ItemId { get; init; }
    public required bool IsHq { get; init; }
    public MarketBoardItemPrice? NqPrice { get; init; }
    public MarketBoardItemPrice? HqPrice { get; init; }

    public DateTime LastUpdatedAt { get; init; }

    public bool IsExpired => DateTime.UtcNow - LastUpdatedAt > CacheDuration;

    public uint GetPrice()
    {
        return AWC.Config.ItemFilters.ItemPriceType.GetPrice(IsHq ? HqPrice : NqPrice);
    }

    public override string ToString()
    {
        return $"MarketBoardItem=[IsLoaded={IsLoaded}, ItemId={ItemId}, IsHq={IsHq}, NqPrice={NqPrice}, HqPrice={HqPrice}, LastUpdatedAt={LastUpdatedAt}]";
    }
}

public sealed class MarketBoardItemPrice
{
    public required MarketBoardItemPriceList MinListing { get; init; }
    public required MarketBoardItemPriceList RecentListing { get; init; }
}

public sealed class MarketBoardItemPriceList
{
    public required uint WorldPrice { get; init; }
    public required uint DcPrice { get; init; }
    public required uint RegionPrice { get; init; }
}
