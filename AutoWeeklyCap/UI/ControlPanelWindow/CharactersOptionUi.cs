using AutoWeeklyCap.IPC.AutoRetainer;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface;

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class CharactersOptionUi
{
    public static void Draw()
    {
        Card.Draw("Characters", DrawCharacterList, collapsible: false);
        DrawCharacterImporter();
    }

    private static void DrawCharacterList()
    {
        ImGui.TextWrapped("Lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum, lorem ipsum");

        Card.Separator();

        foreach (var characterAndWorld in AWC.Config.GetSortedCharacters()) {
            var option = AWC.Config.Characters[characterAndWorld];

            ImGui.PushID(characterAndWorld);

            CharacterElements.DrawCharacterVisibilityIcon(characterAndWorld, option);
            CharacterElements.DrawCharacterStatusIcon(characterAndWorld, option, true);
            CharacterElements.DrawCharacterPositionIcons(characterAndWorld, option, true);
            CharacterElements.DrawCharacterSettingsIcon(characterAndWorld, true);

            DrawCharacterDetails(characterAndWorld);

            ImGui.PopID();
        }
    }

    private static void DrawCharacterImporter()
    {
        if (!AutoRetainerIPC.IsEnabled) {
            return;
        }

        try {
            Dictionary<ulong, string> characterNames = [];
            foreach (var registeredCharacter in AutoRetainerIPC.GetRegisteredCharacters()) {
                OfflineCharacterData character = AutoRetainerIPC.GetOfflineCharacterData(registeredCharacter);

                if (!AWC.Config.Characters.ContainsKey(character.ToString())) {
                    characterNames.Add(character.CID, character.ToString());
                }
            }

            if (characterNames.Count == 0) {
                return;
            }

            DrawCharacterImporterCard(characterNames);
        } catch (Exception) {
            // ignored
        }
    }

    private static void DrawCharacterImporterCard(Dictionary<ulong, string> characterNames)
    {
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Spacing();

        Card.DrawSubtle("Import Characters via AutoRetainer", () =>
        {
            ImGui.Text("The follow characters have been detected within AutoRetainer and are missing");
            ImGui.Text("from AWC, you can click on the plus icon to add the characters.");

            ImGui.Spacing();

            foreach (var (id, name) in characterNames) {
                using (Theme.PushSuccessButton()) {
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, $"AddCharacterViaAutoRetainer:{name}")) {
                        AWC.Config.GetOrRegisterCharacterOptions(id, name);
                    }
                }

                ImGuiEx.Tooltip($"Add {name} to AWC");

                DrawCharacterDetails(name, 18);
            }
        }, id: "auto-retainer-character-importer");
    }

    private static void DrawCharacterDetails(string character, float padding = 8)
    {
        ImGui.SameLine(0f, 4f);

        var cursorPos = ImGui.GetCursorPos();
        ImGui.ProgressBar(0, new Vector2(ImGui.GetContentRegionAvail().X - padding, ImGui.GetFrameHeight()), "");
        ImGui.SameLine();

        cursorPos.X += 8;
        ImGui.SetCursorPos(cursorPos);

        ImGui.TextWrapped(character);
    }
}
