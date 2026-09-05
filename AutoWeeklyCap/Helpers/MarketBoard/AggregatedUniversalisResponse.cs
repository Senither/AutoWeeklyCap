using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global

namespace AutoWeeklyCap.Helpers.MarketBoard;

public class AggregatedUniversalisResponse
{
    [JsonPropertyName("results")] public List<Result> Results { get; init; } = [];
    [JsonPropertyName("failedItems")] public List<object> FailedItems { get; init; } = [];
}

public class Result
{
    [JsonPropertyName("itemId")] public int ItemId { get; set; }
    [JsonPropertyName("nq")] public QualityData Nq { get; set; } = new();
    [JsonPropertyName("hq")] public QualityData Hq { get; set; } = new();
}

public class QualityData
{
    [JsonPropertyName("minListing")] public MarketData MinListing { get; set; } = new();
    [JsonPropertyName("recentPurchase")] public MarketData RecentPurchase { get; set; } = new();
    [JsonPropertyName("averageSalePrice")] public MarketData AverageSalePrice { get; set; } = new();
}

public class MarketData
{
    [JsonPropertyName("world")] public MarketEntry? World { get; set; }
    [JsonPropertyName("dc")] public MarketEntry? Dc { get; set; }
    [JsonPropertyName("region")] public MarketEntry? Region { get; set; }
}

public class MarketEntry
{
    [JsonPropertyName("price")] public double? Price { get; set; }
    [JsonPropertyName("timestamp")] public long? Timestamp { get; set; }
    [JsonPropertyName("worldId")] public int? WorldId { get; set; }
    [JsonPropertyName("quantity")] public double? Quantity { get; set; }
}
