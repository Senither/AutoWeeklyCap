using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class StopActionsUi
{
    public static void Draw()
    {
        ImGui.TextWrapped("Select what should happen when all characters have been tomestone capped.");

        ImGui.Spacing();
        ImGui.Spacing();

        foreach (StopAction action in Enum.GetValues(typeof(StopAction))) {
            if (ImGui.RadioButton(action.GetName(), AWC.Config.StopAction == action)) {
                AWC.Config.StopAction = action;
            }

            var tooltip = action.GetTooltip();
            if (tooltip != null) {
                InformationTooltip.Draw(tooltip);
            }
        }

        ImGui.Spacing();
        ImGui.Spacing();

        Disabled.Draw(!IsDrawCharacterSwitchEnabled(AWC.Config.StopAction), DrawCharacterSwitch);
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
                AWC.Config.Characters.ContainsKey(AWC.Config.CharacterForSwap)
                    ? AWC.Config.CharacterForSwap
                    : "Not selected"
            )) {
            foreach (var character in AWC.Config.GetSortedCharacters()) {
                if (ImGui.Selectable(character, AWC.Config.CharacterForSwap == character)) {
                    AWC.Config.CharacterForSwap = character;
                }
            }

            ImGui.EndCombo();
        }
    }
}
