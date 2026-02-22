using AutoWeeklyCap.Config;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Interface;

namespace AutoWeeklyCap.UI.ConfigWindow;

public static class CharactersOptionUi
{
    public static void Draw()
    {
        foreach (var characterAndWorld in AWC.Config.GetSortedCharacters())
        {
            var option = AWC.Config.Characters[characterAndWorld];

            ImGui.PushID(characterAndWorld);

            CharacterElements.DrawCharacterVisibilityIcon(characterAndWorld, option);
            CharacterElements.DrawCharacterStatusIcon(characterAndWorld, option, sameLine: true);
            CharacterElements.DrawCharacterSettingsIcon(characterAndWorld, sameLine: true);

            DrawCharacterDetails(characterAndWorld);

            ImGui.PopID();
        }
    }

    private static void DrawCharacterDetails(string character)
    {
        ImGui.SameLine(0f, 4f);

        var cursorPos = ImGui.GetCursorPos();
        ImGui.ProgressBar(0, new Vector2(ImGui.GetContentRegionAvail().X - 8, ImGui.GetFrameHeight()), "");
        ImGui.SameLine();

        cursorPos.X += 8;
        ImGui.SetCursorPos(cursorPos);

        ImGui.TextWrapped(character);
    }
}
