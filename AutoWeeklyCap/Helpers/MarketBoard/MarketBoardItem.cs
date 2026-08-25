namespace AutoWeeklyCap.Helpers.MarketBoard;

public class MarketBoardItem
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(3);

    public bool IsLoaded { get; init; }

    public uint ItemId { get; init; }
    public uint NqPrice { get; init; }
    public uint HqPrice { get; init; }

    public DateTime LastUpdatedAt { get; init; }

    public bool IsExpired => DateTime.UtcNow - LastUpdatedAt > CacheDuration;

    public override string ToString()
    {
        return $"MarketBoardItem=[IsLoaded={IsLoaded}, ItemId={ItemId}, NqPrice={NqPrice}, HqPrice={HqPrice}, LastUpdatedAt={LastUpdatedAt}]";
    }
}
