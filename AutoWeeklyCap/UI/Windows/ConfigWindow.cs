using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using ECommons.Configuration;
using ECommons.ImGuiMethods;
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
        }

        var zoneName = Utils.GetZoneNameFromId(Plugin.Config.ZoneId);
        if (zoneName != null)
        {
            ImGui.Text($"Selected duty: {zoneName}");
        }
        else
        {
            ImGui.Text("Invalid territory.");
        }

        var stopGracefully = Plugin.Config.StopRunnerGracefully;
        if (ImGui.Checkbox("Stop gracefully", ref stopGracefully))
        {
            Plugin.Config.StopRunnerGracefully = stopGracefully;
        }

        ImGuiEx.Tooltip(
            "When this is enabled and the runner is stopped, it will finish the AutoDuty run before stopping AutoDuty itself.");
    }

    public override void OnClose()
    {
        Plugin.Config.Save();
    }
}
