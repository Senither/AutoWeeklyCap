using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin) : base("Auto Weekly Tomestone Capper##main-window")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.Button("Manage Characters"))
        {
            plugin.ToggleConfigUi();
        }
        
        // unsafe
        // {
        //     var count = InventoryManager.Instance()->GetWeeklyAcquiredTomestoneCount();
        //     var weeklyCap = InventoryManager.GetLimitedTomestoneWeeklyLimit();
        //
        //     DuoLog.Information("Weekly tombstones: " + count);
        //     DuoLog.Information("Weekly cap limit: " + weeklyCap);
        //     DuoLog.Information("Weekly cap left: " + (weeklyCap - count));
        // }
        //
        // DuoLog.Information("Lifestream is enabled: " + LifestreamIPC.IsEnabled);
        // DuoLog.Information("Lifestream is busy: " + LifestreamIPC.IsBusy.Invoke());
        // DuoLog.Information("Relogging to alt: " + LifestreamIPC.ChangeCharacter("Zenith Ether","Raiden"));

        ImGui.Spacing();

        using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
        {
            // Check if this child is drawing
            if (child.Success)
            {
                ImGuiHelpers.ScaledDummy(20.0f);

                // Example for other services that Dalamud provides.
                // PlayerState provides a wrapper filled with information about the player character.

                var playerState = Plugin.PlayerState;
                if (!playerState.IsLoaded)
                {
                    ImGui.Text("Our local player is currently not logged in.");
                    return;
                }

                if (!playerState.ClassJob.IsValid)
                {
                    ImGui.Text("Our current job is currently not valid.");
                    return;
                }

                // If you want to see the Macro representation of this SeString use `.ToMacroString()`
                // More info about SeStrings: https://dalamud.dev/plugin-development/sestring/
                ImGui.Text(
                    $"Our current job is ({playerState.ClassJob.RowId}) '{playerState.ClassJob.Value.Abbreviation}' with level {playerState.Level}");

                // Example for querying Lumina, getting the name of our current area.
                var territoryId = Plugin.ClientState.TerritoryType;
                if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(plugin.Configuration.ZoneId, out var territoryRow))
                {
                    ImGui.Text($"We are currently in ({plugin.Configuration.ZoneId}) '{territoryRow.PlaceName.Value.Name}'");
                }
                else
                {
                    ImGui.Text("Invalid territory.");
                }
            }
        }
    }
}
