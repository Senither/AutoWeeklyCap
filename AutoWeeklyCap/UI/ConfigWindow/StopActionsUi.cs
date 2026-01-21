using System;
using AutoWeeklyCap.Actions;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Bindings.ImGui;

namespace AutoWeeklyCap.UI.ConfigWindow;

public static class StopActionsUi
{
    public static void Draw()
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

            var tooltip = action.GetTooltip();
            if (tooltip != null)
                InformationTooltip.Draw(tooltip);
        }

        ImGui.Spacing();
        ImGui.Spacing();

        Disabled.Draw(!IsDrawCharacterSwitchEnabled(AutoWeeklyCap.Config.StopAction), DrawCharacterSwitch);
    }

    private static bool IsDrawCharacterSwitchEnabled(StopAction action)
    {
        return action is StopAction.SwitchCharacter or StopAction.StartUnlimitedRuns;
    }

    private static void DrawCharacterSwitch()
    {
        ImGui.TextWrapped("Preferred Character");

        if (ImGui.BeginCombo(
                $"###character-selector",
                AutoWeeklyCap.Config.Characters.ContainsKey(AutoWeeklyCap.Config.CharacterForSwap)
                    ? AutoWeeklyCap.Config.CharacterForSwap
                    : "Not selected"
            ))
        {
            foreach (var character in AutoWeeklyCap.Config.GetSortedCharacters())
            {
                if (ImGui.Selectable(character, AutoWeeklyCap.Config.CharacterForSwap == character))
                {
                    AutoWeeklyCap.Config.CharacterForSwap = character;
                }
            }

            ImGui.EndCombo();
        }
    }
}
