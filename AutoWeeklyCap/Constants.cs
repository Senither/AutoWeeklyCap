namespace AutoWeeklyCap;

public static class Constants
{
    internal const string Name = "Auto Weekly Cap";
    internal const string InternalName = "AutoWeeklyCap";

    internal const string CommandNameShort = "/awc";
    internal const string CommandNameLong = "/autoweeklycap";

    internal const int CurrentMaxLevel = 100;
    internal const int LimitedCurrencyCap = 2000;
    internal const int CurrentMaxItemLevel = 795;

    internal const int LimitedTomesPerRun = 50;
    internal const int UncappedTomesPerRun = 80;

    // Orange Juice
    internal const uint LevelingFoodItemId = 4745;

    // Metrics keys
    internal const string MetricUncappedAcquiredTomestoneKey = "UncappedAcquiredTomestone";
    internal const string MetricWeeklyAcquiredLimitedTomestoneKey = "WeeklyAcquiredLimitedTomestone";
}
