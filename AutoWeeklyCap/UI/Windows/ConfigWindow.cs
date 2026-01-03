using System;
using System.Numerics;
using AutoWeeklyCap.Actions;
using AutoWeeklyCap.Helpers;
using AutoWeeklyCap.Runner;
using AutoWeeklyCap.UI.ConfigWindow;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
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
        var hasHiddenCharacters = false;
        foreach (var option in AutoWeeklyCap.Config.Characters.Values)
        {
            if (option.IsHidden())
            {
                hasHiddenCharacters = true;
            }
        }

        Card.Draw("Duty Options", DrawDutyOptions);

        if (hasHiddenCharacters)
            Card.Draw("Hidden Characters", DrawHiddenCharacters);

        Card.Draw("Between Runs Options", BetweenRunOptionsUi.Draw);
        
        Card.Draw("Stop Actions", DrawStopActions);
        Card.DrawWarning("Manually reset Tomestones", DrawResetWeeklyTomestones);
    }

    private static void DrawDutyOptions()
    {
        ImGui.TextWrapped("Selected duty");

        if (ImGui.BeginCombo(
                $"###selected-duty",
                TomestoneZone.IsSupportedTomestoneZone(AutoWeeklyCap.Config.ZoneId)
                    ? Utils.GetZoneNameFromId(AutoWeeklyCap.Config.ZoneId)
                    : "Not selected"
            ))
        {
            foreach (var zoneId in TomestoneZone.AvailableTomestoneZones)
            {
                if (ImGui.Selectable(Utils.GetZoneNameFromId(zoneId), AutoWeeklyCap.Config.ZoneId == zoneId))
                {
                    AutoWeeklyCap.Config.ZoneId = zoneId;
                }
            }

            ImGui.EndCombo();
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

        var useBossModRebornAI = AutoWeeklyCap.Config.UseBossModRebornAI;
        if (ImGui.Checkbox("Use BossMod Reborn AI", ref useBossModRebornAI))
        {
            AutoWeeklyCap.Config.UseBossModRebornAI = useBossModRebornAI;
        }

        InformationTooltip.Draw("When enabled, the BossMod Reborn AI will be used for AutoDuty over the default AI");
    }

    private static void DrawHiddenCharacters()
    {
        ImGui.TextWrapped("Hidden characters don't show in the character list, and are ignored for tomestone runs.");

        ImGui.Spacing();
        ImGui.Spacing();

        foreach (var (characterAndWorld, options) in AutoWeeklyCap.Config.Characters)
        {
            if (!options.IsHidden())
                continue;

            if (ImGuiEx.IconButton(FontAwesomeIcon.Eye, "###show-hidden-character" + characterAndWorld))
            {
                options.Hidden = false;
            }

            ImGui.SameLine();
            ImGui.TextWrapped(characterAndWorld);
        }
    }

    private static void DrawStopActions()
    {
        ImGui.TextWrapped("Select what should happen when all characters have been tomestone capped.");

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
                AutoWeeklyCap.Config.Characters.ContainsKey(AutoWeeklyCap.Config.CharacterForSwap)
                    ? AutoWeeklyCap.Config.CharacterForSwap
                    : "Not selected"
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

    private static void DrawResetWeeklyTomestones()
    {
        ImGui.TextWrapped(
            "The tomestones will reset automatically during the weekly reset, however, " +
            "if you want to reset the tomes manually you can use the button below."
        );

        ImGui.Spacing();
        ImGui.Spacing();

        ActionButton.Draw(
            "Reset Weekly Tomestones",
            "Hold down CTRL to reset your weekly tomestones",
            () => AutoWeeklyCap.Config.CollectedTomes.Clear()
        );
    }

    public override void OnClose()
    {
        AutoWeeklyCap.Config.Save();
    }
}
