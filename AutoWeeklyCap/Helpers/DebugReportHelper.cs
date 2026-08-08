using AutoWeeklyCap.Config;

using Dalamud.Plugin;

using ECommons.Configuration;
using ECommons.Logging;

namespace AutoWeeklyCap.Helpers;

public static class DebugReportHelper
{
    public static string? GenerateReport(bool prettyPrint = false)
    {
        return EzConfig.DefaultSerializationFactory.Serialize(
            prettyPrint: prettyPrint,
            config: new PluginDebugInformation(
                PluginVersion: AWC.Version,
                RunnerState: AWC.Runner.State,
                Configuration: AWC.Config,
                Player: GetPlayerDetails(),
                HelperState: GetHelperState(),
                InstalledPlugins: GetInstalledPlugins(),
                LogMessages: InternalLog.Messages.ToArray()
            )
        );
    }

    private static PlayerDetails? GetPlayerDetails()
    {
        if (!Player.Available) {
            return null;
        }

        return new PlayerDetails(
            Player.CID,
            Player.NameWithWorld,
            Player.CurrentWorldName,
            Player.Position,
            Player.Territory.RowId,
            Player.ClassJob.RowId,
            Player.Level
        );
    }

    private static Dictionary<string, object> GetHelperState()
    {
        Dictionary<string, object> state = new();
        if (!Player.Available) {
            return state;
        }

        state["CurrencyHelper#GetGil"] = CurrencyHelper.GetGil();
        state["CurrencyHelper#GetUncappedAcquiredTomestoneCount"] = CurrencyHelper.GetUncappedAcquiredTomestoneCount();
        state["CurrencyHelper#IsPlayerLimitedTomestoneCapped"] = CurrencyHelper.IsPlayerLimitedTomestoneCapped();
        state["CurrencyHelper#IsPlayerWeeklyLimitedTomestoneCapped"] = CurrencyHelper.IsPlayerWeeklyLimitedTomestoneCapped();
        state["CurrencyHelper#IsPlayerTotalLimitedTomestoneCapped"] = CurrencyHelper.IsPlayerTotalLimitedTomestoneCapped();
        state["CurrencyHelper#GetLimitedTomestoneWeeklyLimit"] = CurrencyHelper.GetLimitedTomestoneWeeklyLimit();
        state["CurrencyHelper#GetWeeklyAcquiredLimitedTomestoneCount"] = CurrencyHelper.GetWeeklyAcquiredLimitedTomestoneCount();
        state["CurrencyHelper#GetTotalAcquiredLimitedTomestoneCount"] = CurrencyHelper.GetTotalAcquiredLimitedTomestoneCount();
        state["GrandCompanyHelper#GetGrandCompany"] = GrandCompanyHelper.GetGrandCompany();
        state["HousingHelper#IsInsideHouse"] = HousingHelper.IsInsideHouse();
        state["HousingHelper#IsInsideApartment"] = HousingHelper.IsInsideApartment();
        state["HousingHelper#IsInsideFC"] = HousingHelper.IsInsideFC();
        state["LevelingHelper#GetCharacterToLevel"] = LevelingHelper.GetCharacterToLevel() ?? "<empty>";
        state["PlayerHelper#IsReady"] = PlayerHelper.IsReady;
        state["PlayerHelper#IsLoggedIn"] = PlayerHelper.IsLoggedIn;
        state["PlayerHelper#IsOccupied"] = PlayerHelper.IsOccupied;
        state["PlayerHelper#IsValid"] = PlayerHelper.IsValid;
        state["PlayerHelper#CanSelfRepairWithCrafters"] = PlayerHelper.CanSelfRepairWithCrafters;
        state["InventoryHelper#GetEmptySlotsInBag"] = InventoryHelper.GetEmptySlotsInBag();
        state["InventoryHelper#IsAtleastOneArmoryChestSlotFull"] = InventoryHelper.IsAtleastOneArmoryChestSlotFull();
        state["InventoryHelper#GetDeliverableItemsCount"] = InventoryHelper.GetDeliverableItemsCount();
        state["InventoryHelper#GetLowestConditionEquippedItem"] = InventoryHelper.GetLowestConditionEquippedItem().Container;
        state["InventoryHelper#GetDarkMatterCount"] = InventoryHelper.GetDarkMatterCount();
        state["InventoryHelper#GetCurrentItemLevel"] = InventoryHelper.GetCurrentItemLevel();

        return state;
    }

    private static Dictionary<string, bool> GetInstalledPlugins()
    {
        Dictionary<string, bool> plugins = new Dictionary<string, bool>();

        foreach (IExposedPlugin plugin in AWC.PluginInterface.InstalledPlugins) {
            plugins[plugin.Name] = plugin.IsLoaded;
        }

        return plugins;
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
    private record PluginDebugInformation(
        string PluginVersion,
        RunnerState RunnerState,
        Configuration Configuration,
        PlayerDetails? Player,
        Dictionary<string, object> HelperState,
        Dictionary<string, bool> InstalledPlugins,
        InternalLogMessage[] LogMessages
    );

    private record PlayerDetails(
        // ReSharper disable once InconsistentNaming
        ulong CID,
        string Name,
        string CurrentWorld,
        Vector3 Position,
        uint Territory,
        uint ClassJob,
        int ClassLevel
    );
    // ReSharper restore NotAccessedPositionalProperty.Local
}
