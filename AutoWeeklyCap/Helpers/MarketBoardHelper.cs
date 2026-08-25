using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using AutoWeeklyCap.Helpers.MarketBoard;

namespace AutoWeeklyCap.Helpers;

public static class MarketBoardHelper
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly Dictionary<uint, MarketBoardItem> ItemsCache = new();
    private static readonly SemaphoreSlim FetchLock = new(1, 1);

    public static async Task<List<MarketBoardItem>> GetMarketBoardPrices(uint serverId, HashSet<uint> uniqueItemIds)
    {
        List<MarketBoardItem> result = [];
        HashSet<uint> itemIdsToFetch = [];

        foreach (var itemId in uniqueItemIds) {
            if (ItemsCache.TryGetValue(itemId, out var cachedItem) && !cachedItem.IsExpired) {
                result.Add(cachedItem);
            } else {
                itemIdsToFetch.Add(itemId);
            }
        }

        if (itemIdsToFetch.Count == 0) {
            return result;
        }

        AggregatedUniversalisResponse? response;

        await FetchLock.WaitAsync();

        try {
            response = await FetchAggregatedPricesFromUniversalis(serverId, itemIdsToFetch);
        } catch (Exception) {
            return result;
        } finally {
            FetchLock.Release();
        }

        foreach (var itemId in itemIdsToFetch) {
            MarketBoardItem marketBoardItem = BuildMarketBoardItem(itemId, response);

            ItemsCache[itemId] = marketBoardItem;
            result.Add(marketBoardItem);
        }

        return result;
    }

    private static MarketBoardItem BuildMarketBoardItem(uint itemId, AggregatedUniversalisResponse? response)
    {
        Result? result = response?.Results.Find(r => r.ItemId == itemId);

        if (result is null) {
            return new MarketBoardItem { ItemId = itemId, IsLoaded = false };
        }

        return new MarketBoardItem
        {
            IsLoaded = true,
            ItemId = itemId,
            NqPrice = GetHighestPrice(result.Nq),
            HqPrice = GetHighestPrice(result.Hq),
            LastUpdatedAt = DateTime.UtcNow,
        };
    }

    private static uint GetHighestPrice(QualityData qualityData)
    {
        double worldPrice = qualityData.RecentPurchase.World?.Price ?? 0;
        double dcPrice = qualityData.RecentPurchase.Dc?.Price ?? 0;

        return (uint)Math.Max(worldPrice, dcPrice);
    }

    private static async Task<AggregatedUniversalisResponse?> FetchAggregatedPricesFromUniversalis(
        uint serverId,
        HashSet<uint> uniqueItemIds,
        CancellationToken cancellationToken = default
    )
    {
        AWC.Log.Debug($"[MarketBoardHelper] Sending request to Universalis with IDs ({string.Join(",", uniqueItemIds)})");

        using var response = await BuildHttpClient(serverId).GetAsync(string.Join(",", uniqueItemIds), cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await JsonSerializer.DeserializeAsync<AggregatedUniversalisResponse>(
            stream,
            JsonSerializerOptions,
            cancellationToken
        );
    }

    private static HttpClient BuildHttpClient(uint serverId)
    {
        HttpClient client = new HttpClient();

        client.Timeout = TimeSpan.FromSeconds(30);
        client.BaseAddress = new Uri($"https://universalis.app/api/v2/aggregated/{serverId}/");

        client.DefaultRequestHeaders.UserAgent.ParseAdd($"AutoWeeklyCap/{AWC.Version}");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }
}
