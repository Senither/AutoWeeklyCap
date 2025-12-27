using System;
using System.Numerics;
using AutoWeeklyCap.Actions;
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

        foreach (StopAction action in Enum.GetValues(typeof(StopAction)))
        {
            if (ImGui.RadioButton(action.GetName(), AutoWeeklyCap.Config.StopAction == action))
            {
                AutoWeeklyCap.Config.StopAction = action;
            }
        }

        if (AutoWeeklyCap.Config.StopAction != StopAction.SwitchCharacter)
            ImGui.BeginDisabled();

        if (ImGui.BeginCombo(
                $"Character to swap to",
                AutoWeeklyCap.Config.CharacterForSwap.Length == 0
                    ? "Not selected"
                    : AutoWeeklyCap.Config.CharacterForSwap
            ))
        {
            foreach (var character in AutoWeeklyCap.Config.Characters.Keys)
            {
                if (ImGui.Selectable(character, AutoWeeklyCap.Config.CharacterForSwap == character))
                {
                    AutoWeeklyCap.Config.CharacterForSwap = character;
                }
            }

            ImGui.EndCombo();
        }

        if (AutoWeeklyCap.Config.StopAction != StopAction.SwitchCharacter)
            ImGui.EndDisabled();
    }

    public override void OnClose()
    {
        AutoWeeklyCap.Config.Save();
    }
}
