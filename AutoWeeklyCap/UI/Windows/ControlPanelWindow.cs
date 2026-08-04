using AutoWeeklyCap.Config;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;

using ECommons.Configuration;

namespace AutoWeeklyCap.UI.Windows;

public class ControlPanelWindow : Window
{
    private SettingsWindowOption _option = SettingsWindowOption.GeneralOptions;

    public ControlPanelWindow() : base("Auto Weekly Cap Control Panel")
    {
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(575, 400), MaximumSize = new Vector2(9999, 9999) };

        // @formatter:off
        TitleBarButtons.Add(new TitleBarButton
        {
            Click = _ => CopyDebugReportToClipboard(),
            Icon = FontAwesomeIcon.Code,
            IconOffset = new Vector2(2, 2),
            ShowTooltip = () => ImGui.SetTooltip("Copies debug and diagnostics plugin information to clipboard")
        });
        // @formatter:on
    }

    public override void Draw()
    {
        using (Theme.Push()) {
            SidebarLayout.DrawSidebar(() =>
            {
                foreach (var option in Enum.GetValues(typeof(SettingsWindowOption)).Cast<SettingsWindowOption>()) {
                    if (!option.IsDrawable()) {
                        continue;
                    }

                    if (MenuButton.Draw(option.GetIcon(), option.GetName(), _option == option, widthBreakpoint: SidebarLayout.GetSidebarContentTextBreakpoint())) {
                        SetCurrentTab(option);
                    }
                }
            });

            SidebarLayout.DrawContent(() => _option.Draw());
        }
    }

    public void SetCurrentTab(SettingsWindowOption option)
    {
        _option = option;
    }

    public override void OnClose()
    {
        EzConfig.Save();
    }

    private static void CopyDebugReportToClipboard()
    {
        var installedPlugins = new Dictionary<string, bool>();
        foreach (IExposedPlugin plugin in AWC.PluginInterface.InstalledPlugins) {
            installedPlugins[plugin.Name] = plugin.IsLoaded;
        }

        Dictionary<string, object> helperState = new();
        PlayerDetails? player = null;

        if (Player.Available) {
            helperState["CurrencyHelper#GetGil"] = CurrencyHelper.GetGil();
            helperState["CurrencyHelper#GetUncappedAcquiredTomestoneCount"] = CurrencyHelper.GetUncappedAcquiredTomestoneCount();
            helperState["CurrencyHelper#IsPlayerLimitedTomestoneCapped"] = CurrencyHelper.IsPlayerLimitedTomestoneCapped();
            helperState["CurrencyHelper#IsPlayerWeeklyLimitedTomestoneCapped"] = CurrencyHelper.IsPlayerWeeklyLimitedTomestoneCapped();
            helperState["CurrencyHelper#IsPlayerTotalLimitedTomestoneCapped"] = CurrencyHelper.IsPlayerTotalLimitedTomestoneCapped();
            helperState["CurrencyHelper#GetLimitedTomestoneWeeklyLimit"] = CurrencyHelper.GetLimitedTomestoneWeeklyLimit();
            helperState["CurrencyHelper#GetWeeklyAcquiredLimitedTomestoneCount"] = CurrencyHelper.GetWeeklyAcquiredLimitedTomestoneCount();
            helperState["CurrencyHelper#GetTotalAcquiredLimitedTomestoneCount"] = CurrencyHelper.GetTotalAcquiredLimitedTomestoneCount();
            helperState["GrandCompanyHelper#GetGrandCompany"] = GrandCompanyHelper.GetGrandCompany();
            helperState["HousingHelper#IsInsideHouse"] = HousingHelper.IsInsideHouse();
            helperState["HousingHelper#IsInsideApartment"] = HousingHelper.IsInsideApartment();
            helperState["HousingHelper#IsInsideFC"] = HousingHelper.IsInsideFC();
            helperState["LevelingHelper#GetCharacterToLevel"] = LevelingHelper.GetCharacterToLevel() ?? "<empty>";
            helperState["PlayerHelper#IsReady"] = PlayerHelper.IsReady;
            helperState["PlayerHelper#IsLoggedIn"] = PlayerHelper.IsLoggedIn;
            helperState["PlayerHelper#IsOccupied"] = PlayerHelper.IsOccupied;
            helperState["PlayerHelper#IsValid"] = PlayerHelper.IsValid;
            helperState["PlayerHelper#CanSelfRepairWithCrafters"] = PlayerHelper.CanSelfRepairWithCrafters;
            helperState["InventoryHelper#GetEmptySlotsInBag"] = InventoryHelper.GetEmptySlotsInBag();
            helperState["InventoryHelper#IsAtleastOneArmoryChestSlotFull"] = InventoryHelper.IsAtleastOneArmoryChestSlotFull();
            helperState["InventoryHelper#GetDeliverableItemsCount"] = InventoryHelper.GetDeliverableItemsCount();
            helperState["InventoryHelper#GetLowestConditionEquippedItem"] = InventoryHelper.GetLowestConditionEquippedItem().Container;
            helperState["InventoryHelper#GetDarkMatterCount"] = InventoryHelper.GetDarkMatterCount();
            helperState["InventoryHelper#GetCurrentItemLevel"] = InventoryHelper.GetCurrentItemLevel();

            player = new PlayerDetails(
                Player.CID,
                Player.NameWithWorld,
                Player.CurrentWorldName,
                Player.Position,
                Player.Territory.RowId,
                Player.ClassJob.RowId,
                Player.Level
            );
        }

        ImGui.SetClipboardText(EzConfig.DefaultSerializationFactory.Serialize(new PluginDebugInformation(
            AWC.Version,
            AWC.Runner.State,
            AWC.Config,
            player,
            helperState,
            installedPlugins
        )));

        Notify.Info("Debug & Diagnostics info has been copied to clipboard");
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
    private record PluginDebugInformation(
        string PluginVersion,
        RunnerState RunnerState,
        Configuration Configuration,
        PlayerDetails? Player,
        Dictionary<string, object> HelperState,
        Dictionary<string, bool> InstalledPlugins
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
