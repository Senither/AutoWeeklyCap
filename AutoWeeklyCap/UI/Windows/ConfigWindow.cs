using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("Auto Weekly Tomestone Settings")
    {
        Flags = ImGuiWindowFlags.NoResize;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(350, 420),
            MaximumSize = new Vector2(350, 420)
        };

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var zoneId = configuration.ZoneId;
        if (ImGui.InputUInt("Duty ID", ref zoneId))
        {
            configuration.ZoneId = zoneId;
            configuration.Save();
        }

        if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(configuration.ZoneId, out var territoryRow))
        {
            ImGui.Text($"Selected duty: {territoryRow.PlaceName.Value.Name}");
        }
        else
        {
            ImGui.Text("Invalid territory.");
        }

        ImGui.Spacing();
        ImGui.Spacing();

        for (var i = 0; i < 9; i++)
        {
            DrawCharacterInput(i);
        }

        ImGui.Text("The characters must be in the format:");
        ImGui.Text("    FirstName LastName@Server");
        ImGui.TextWrapped(
            "If the character name is incorrectly formatted, Lifestream can enter a login loop when trying to relog.");
    }

    protected void DrawCharacterInput(int index)
    {
        var character = configuration.Characters[index];
        if (ImGui.InputText("Character " + (index + 1), ref character))
        {
            configuration.Characters[index] = character;
            configuration.Save();
        }
    }
}
