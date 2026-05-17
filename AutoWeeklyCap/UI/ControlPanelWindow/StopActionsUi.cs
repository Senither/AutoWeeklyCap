using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class StopActionsUi
{
    public static void Draw()
    {
        Card.Draw("Stop Actions", () =>
        {
            ImGui.TextWrapped("Select what should happen when all characters have been tomestone capped.");

            Card.Separator();

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
        }, collapsible: false);

        List<Action> elements = GetOptionsDrawElements();
        if (elements.Count > 0) {
            Card.Draw("Stop Options", () =>
            {
                foreach (var element in elements) {
                    element.Invoke();
                }
            }, collapsible: false);
        } else {
            ImGui.Spacing();
            ImGui.Spacing();

            Disabled.Draw(() => ImGui.TextWrapped(
                "Selecting certain options will show additional options here, allowing you to further customize the behaviour of the actions."
            ));
        }
    }

    private static List<Action> GetOptionsDrawElements()
    {
        return AWC.Config.StopAction switch
        {
            StopAction.SwitchCharacter => [DrawCharacterSwitch],
            StopAction.StartUnlimitedRuns => [DrawCharacterSwitch],
            _ => []
        };
    }

    private static void DrawCharacterSwitch()
    {
        ImGui.TextWrapped("Preferred Character");

        var preview = AWC.Config.Characters.ContainsKey(AWC.Config.CharacterForSwap)
            ? AWC.Config.CharacterForSwap
            : "Not selected";

        if (!ImGui.BeginCombo($"###character-selector", preview)) {
            return;
        }

        foreach (var character in AWC.Config.GetSortedCharacters()) {
            if (ImGui.Selectable(character, AWC.Config.CharacterForSwap == character)) {
                AWC.Config.CharacterForSwap = character;
            }
        }

        ImGui.EndCombo();
    }
}
