using System;
using System.Numerics;
using AutoWeeklyCap.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;

namespace AutoWeeklyCap.UI.Windows;

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
        var zoneId = AutoWeeklyCap.Config.ZoneId;
        if (ImGui.InputUInt("Duty ID", ref zoneId))
        {
            AutoWeeklyCap.Config.ZoneId = zoneId;
        }

        var zoneName = Utils.GetZoneNameFromId(AutoWeeklyCap.Config.ZoneId);
        if (zoneName != null)
        {
            ImGui.Text($"Selected duty: {zoneName}");
        }
        else
        {
            ImGui.Text("Invalid territory.");
        }

        var stopGracefully = AutoWeeklyCap.Config.StopRunnerGracefully;
        if (ImGui.Checkbox("Stop gracefully", ref stopGracefully))
        {
            AutoWeeklyCap.Config.StopRunnerGracefully = stopGracefully;
        }

        ImGuiEx.Tooltip(
            "When this is enabled and the runner is stopped, it will finish the AutoDuty run before stopping AutoDuty itself.");
    }

    public override void OnClose()
    {
        AutoWeeklyCap.Config.Save();
    }
}
