using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Windows;

public class ConfigWindow : Window, IDisposable
{
    public ConfigWindow() : base("Auto Weekly Tomestone Settings")
    {
        Flags = ImGuiWindowFlags.NoResize;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(350, 420),
            MaximumSize = new Vector2(350, 420)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        var zoneId = Plugin.Config.ZoneId;
        if (ImGui.InputUInt("Duty ID", ref zoneId))
        {
            Plugin.Config.ZoneId = zoneId;
            Plugin.Config.Save();
        }

        if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(Plugin.Config.ZoneId, out var territoryRow))
        {
            ImGui.Text($"Selected duty: {territoryRow.PlaceName.Value.Name}");
        }
        else
        {
            ImGui.Text("Invalid territory.");
        }
    }
}
