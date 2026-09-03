using System.IO;
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

    private static readonly List<uint> ItemsToExclude =
    [
        21800, // Glamour Prisms
        7621, // Glamour Dispeller
        7671, // Clear Prism
        4868, // Gysahl Greens
        4570, // Phoenix Down
        23168, // Super-Ether
        33916, // Grade 8 Dark Matter

        Constants.LevelingFoodItemId,
    ];

    public static async Task<List<MarketBoardItem>> GetFilteredMarketBoardItemsFromInventory(string dataCenterName)
    {
        HashSet<uint> uniqueItemIds = GetUniqueItemIdsFromInventory();
        if (uniqueItemIds.Count == 0) {
            return [];
        }

        List<MarketBoardItem> marketBoardItems = await GetMarketBoardPricesForUniqueIds(dataCenterName, uniqueItemIds);
        if (marketBoardItems.Count == 0) {
            return [];
        }

        return marketBoardItems
            .Where(item => item.IsLoaded)
            .Select(marketBoardItem =>
            {
                var hasSheetItem = InventoryHelper.TryGetSheetItemFromItemId(marketBoardItem.ItemId, out var sheetItem);
                return new { MarketBoardItem = marketBoardItem, SheetItem = sheetItem, HasSheetItem = hasSheetItem };
            })
            .Where(item => item.MarketBoardItem.GetPrice(item.SheetItem.CanBeHq) < AWC.Config.ItemFilters.GilThreshold)
            .Select(item => item.MarketBoardItem)
            .ToList();
    }

    private static async Task<List<MarketBoardItem>> GetMarketBoardPricesForUniqueIds(string dataCenterName, HashSet<uint> uniqueItemIds)
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
            response = await FetchAggregatedPricesFromUniversalis(dataCenterName, itemIdsToFetch);
        } catch (Exception ex) {
            AWC.Log.Warning($"[{nameof(MarketBoardHelper)}] Failed to fetch aggregated prices from universalis: {ex.Message}\n{ex.StackTrace}");

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
            NqPrice = BuildMarketBoardItemPrice(result.Nq),
            HqPrice = BuildMarketBoardItemPrice(result.Hq),
            LastUpdatedAt = DateTime.UtcNow,
        };
    }

    private static MarketBoardItemPrice BuildMarketBoardItemPrice(QualityData data)
    {
        // @formatter:off
        return new MarketBoardItemPrice {
            MinListing = BuildMarketBoardItemPriceList(data.MinListing),
            RecentListing = BuildMarketBoardItemPriceList(data.RecentPurchase),
            AverageListing = BuildMarketBoardItemPriceList(data.AverageSalePrice)
        };
        // @formatter:on
    }

    private static MarketBoardItemPriceList BuildMarketBoardItemPriceList(MarketData data)
    {
        // @formatter:off
        return new MarketBoardItemPriceList
        {
            WorldPrice = (uint)(data.World?.Price ?? 0u),
            DcPrice =  (uint)(data.Dc?.Price ?? 0u)
        };
        // @formatter:on
    }

    private static async Task<AggregatedUniversalisResponse?> FetchAggregatedPricesFromUniversalis(
        string dataCenterName,
        HashSet<uint> uniqueItemIds,
        CancellationToken cancellationToken = default
    )
    {
        AggregatedUniversalisResponse aggregatedResponse = new();
        using HttpClient client = BuildHttpClient(dataCenterName);

        foreach (uint[] itemIdChunk in uniqueItemIds.Chunk(100)) {
            AWC.Log.Debug($"[{nameof(MarketBoardHelper)}] Sending request to Universalis with IDs ({string.Join(",", itemIdChunk)})");

            using HttpResponseMessage response = await client.GetAsync(string.Join(",", itemIdChunk), cancellationToken);

            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            AggregatedUniversalisResponse? chunkResponse = await JsonSerializer.DeserializeAsync<AggregatedUniversalisResponse>(
                stream,
                JsonSerializerOptions,
                cancellationToken
            );

            if (chunkResponse is null) {
                continue;
            }

            aggregatedResponse.Results.AddRange(chunkResponse.Results);
            aggregatedResponse.FailedItems.AddRange(chunkResponse.FailedItems);
        }

        return aggregatedResponse;
    }

    private static HttpClient BuildHttpClient(string dataCenterName)
    {
        HttpClient client = new HttpClient();

        client.Timeout = TimeSpan.FromSeconds(30);
        client.BaseAddress = new Uri($"https://universalis.app/api/v2/aggregated/{dataCenterName}/");

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

            // Skip items below the item level threshold when value is set to 1 or greater
            if (AWC.Config.ItemFilters.ItemLevelThreshold > 0 && item.LevelItem.RowId >= AWC.Config.ItemFilters.ItemLevelThreshold) {
                continue;
            }

            if (AWC.Config.ItemFilters.ExcludeMateria && item.ItemUICategory.RowId == 58) {
                continue;
            }

            if (AWC.Config.ItemFilters.ExcludeFood && item.ItemUICategory.RowId == 46) {
                continue;
            }

            if (AWC.Config.ItemFilters.ExcludePotions && item.ItemUICategory.RowId == 44) {
                continue;
            }

            if (AWC.Config.ItemFilters.ExcludeDyes && item.ItemUICategory.RowId == 55) {
                continue;
            }

            if (ItemsToExclude.Contains(item.RowId)) {
                continue;
            }

            if (AWC.Config.ItemFilters.BlacklistedItems.Contains(item.RowId)) {
                continue;
            }

            AWC.Log.Debug($"[{nameof(MarketBoardHelper)}] Checking item: {item.Name} | ItemId={inventoryItem.ItemId}, ItemUICategory={item.ItemUICategory.RowId}, ItemSearchCategory={item.ItemSearchCategory.RowId}");

            uniqueItemIds.AddIfNotExist(inventoryItem.ItemId);
        }

        return uniqueItemIds;
    }
}
