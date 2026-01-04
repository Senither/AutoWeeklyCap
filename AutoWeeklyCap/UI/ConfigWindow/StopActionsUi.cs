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
        }

        ImGui.Spacing();
        ImGui.Spacing();

        Disabled.Draw(AutoWeeklyCap.Config.StopAction != StopAction.SwitchCharacter, DrawCharacterSwitch);
    }

    private static void DrawCharacterSwitch()
    {
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
    }
}
