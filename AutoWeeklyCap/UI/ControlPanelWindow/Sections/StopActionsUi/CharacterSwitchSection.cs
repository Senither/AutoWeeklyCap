namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.StopActionsUi;

internal static class CharacterSwitchSection
{
    internal static void Draw()
    {
        ImGui.TextWrapped("Preferred Character");

        string preview = AWC.Config.Characters.ContainsKey(AWC.Config.CharacterForSwap)
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
