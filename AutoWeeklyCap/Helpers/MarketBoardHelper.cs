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

    public static async Task<List<MarketBoardItem>> GetFilteredMarketBoardItemsFromInventory()
    {
        HashSet<uint> uniqueItemIds = GetUniqueItemIdsFromInventory();
        if (uniqueItemIds.Count == 0) {
            return [];
        }

        List<MarketBoardItem> marketBoardItems = await GetMarketBoardPricesForUniqueIds(uniqueItemIds);
        if (marketBoardItems.Count == 0) {
            return [];
        }

        return marketBoardItems
            .Where(item => item is { IsLoaded: true, Price: < 10_000 })
            .OrderBy(item => item.Price)
            .ToList();
    }

    private static async Task<List<MarketBoardItem>> GetMarketBoardPricesForUniqueIds(HashSet<uint> uniqueItemIds)
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
            response = await FetchAggregatedPricesFromUniversalis(itemIdsToFetch);
        } catch (Exception ex) {
            AWC.Log.Error($"[{nameof(MarketBoardHelper)}] Failed to fetch aggregated prices from universalis", ex);

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
        HashSet<uint> uniqueItemIds,
        CancellationToken cancellationToken = default
    )
    {
        AWC.Log.Debug($"[{nameof(MarketBoardHelper)}] Sending request to Universalis with IDs ({string.Join(",", uniqueItemIds)})");

        using var response = await BuildHttpClient().GetAsync(string.Join(",", uniqueItemIds), cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await JsonSerializer.DeserializeAsync<AggregatedUniversalisResponse>(
            stream,
            JsonSerializerOptions,
            cancellationToken
        );
    }

    private static HttpClient BuildHttpClient()
    {
        HttpClient client = new HttpClient();

        client.Timeout = TimeSpan.FromSeconds(30);
        client.BaseAddress = new Uri($"https://universalis.app/api/v2/aggregated/{Player.HomeDataCenterName}/");

        client.DefaultRequestHeaders.UserAgent.ParseAdd($"AutoWeeklyCap/{AWC.Version}");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }

    private static HashSet<uint> GetUniqueItemIdsFromInventory()
    {
        HashSet<uint> uniqueItemIds = [];

        foreach (var inventoryItem in InventoryHelper.GetInventoryItems()) {
            if (!InventoryHelper.TryGetSheetItemFromItemId(inventoryItem.ItemId, out var item)) {
                continue;
            }

            // Skip items with no sell price
            if (item.PriceLow == 0) {
                continue;
            }

            // Skip items that are not sellable on the marketboard
            if (item.ItemSearchCategory.RowId == 0) {
                continue;
            }

            // // Materia
            // if (item.ItemUICategory.RowId is 57) {
            //     continue;
            // }
            //
            // // Glamour Prism & Dispeller & Dark Matter
            // if (item.ItemUICategory.RowId is 60 or 48) {
            //     continue;
            // }
            //
            // // Potions & Food
            // if (item.ItemUICategory.RowId is 60 or 46 or 44) {
            //     continue;
            // }
            //
            // // Gysahl Greens
            // if (item.RowId is 4868) {
            //     continue;
            // }
            //
            // // Minions
            // if (item.ItemUICategory.RowId == 81) {
            //     continue;
            // }
            //
            // // Triple Triad Cards
            // if (item.ItemUICategory.RowId == 86) {
            //     continue;
            // }

            AWC.Log.Debug($"[{nameof(MarketBoardHelper)}] Adding item: {item.Name} | ItemId={inventoryItem.ItemId}, ItemUICategory={item.ItemUICategory.RowId}, ItemSearchCategory={item.ItemSearchCategory.RowId}");

            uniqueItemIds.AddIfNotExist(inventoryItem.ItemId);
        }

        return uniqueItemIds;
    }
}
