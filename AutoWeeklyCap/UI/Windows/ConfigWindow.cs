using System;
using System.Numerics;
using AutoWeeklyCap.Actions;
using AutoWeeklyCap.Helpers;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;

namespace AutoWeeklyCap.UI.Windows;

public class ConfigWindow : Window, IDisposable
{
    public ConfigWindow() : base("Auto Weekly Tomestone Settings")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 125),
            MaximumSize = new Vector2(9999, 9999)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        Card.Draw("Duty Options", DrawDutyOptions);
        Card.Draw("Stop Actions", DrawStopActions);
    }

    private static void DrawDutyOptions()
    {
        ImGui.TextWrapped("Selected duty");

        var zoneId = AutoWeeklyCap.Config.ZoneId;
        if (ImGui.InputUInt("###duty-id", ref zoneId))
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

        ImGui.Spacing();
        ImGui.Spacing();

        var stopGracefully = AutoWeeklyCap.Config.StopRunnerGracefully;
        if (ImGui.Checkbox("Stop runs gracefully", ref stopGracefully))
        {
            AutoWeeklyCap.Config.StopRunnerGracefully = stopGracefully;
        }

        InformationTooltip.Draw(
            "When stopping the runner mid duty, graceful stopping will finish the run before stopping completely");
    }

    private static void DrawStopActions()
    {
        ImGui.TextWrapped("Select what should happen when all characters tomes have been collected.");

        ImGui.Spacing();
        ImGui.Spacing();

        foreach (StopAction action in Enum.GetValues(typeof(StopAction)))
        {
            if (ImGui.RadioButton(action.GetName(), AutoWeeklyCap.Config.StopAction == action))
            {
                AutoWeeklyCap.Config.StopAction = action;
            }
        }

        ImGui.Spacing();
        ImGui.Spacing();

        if (AutoWeeklyCap.Config.StopAction != StopAction.SwitchCharacter)
            ImGui.BeginDisabled();

        ImGui.TextWrapped("Switch to Character");

        if (ImGui.BeginCombo(
                $"###character-selector",
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
