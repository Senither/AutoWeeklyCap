using AutoWeeklyCap.Config;
using AutoWeeklyCap.Runner;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Interface;

namespace AutoWeeklyCap.UI.MainWindow;

internal static class CharactersUI
{
    internal static void Draw()
    {
        var charactersEnabled = 0;
        var totalTomesCollected = 0;
        var weeklyTomeLimit = CurrencyHelper.GetLimitedTomestoneWeeklyLimit();

        foreach (var character in AWC.Config.GetSortedCharacters())
        {
            var option = AWC.Config.Characters[character];
            if (option.IsHidden())
                continue;

            var characterTomes = AWC.Config.GetWeeklyTomes(character);

            if (option.IsEnabled())
                totalTomesCollected += characterTomes;

            if (option.IsEnabled())
                charactersEnabled++;

            ImGui.PushID(character);

            DrawCharacterStatusIcon(character, option);
            DrawCharacterRelogIcon(character);
            DrawCharacterSettingsIcon(character, option);
            DrawCharacterDetails(character, option, characterTomes, weeklyTomeLimit);

            ImGui.PopID();
        }

        ImGuiEx.LineCentered(
            "TomestoneCap",
            () => ImGuiEx.Text($"Weekly tomestone cap is at {totalTomesCollected}/{weeklyTomeLimit * charactersEnabled}"
            )
        );
    }

    internal static void SaveCharacterConfigurationOption(string character, CharacterOptions options)
    {
        AWC.Config.Characters[character] = options;
        AWC.Config.Save();
    }

    internal static void DrawCharacterStatusIcon(string character, CharacterOptions option)
    {
        var isEnabled = option.IsEnabled();
        if (isEnabled)
            ImGui.PushStyleColor(ImGuiCol.Button, 0xFF097000);

        if (ImGuiEx.IconButton(FontAwesomeIcon.Rocket))
        {
            option.Enabled = !isEnabled;
            SaveCharacterConfigurationOption(character, option);
        }

        ImGuiEx.Tooltip($"Click to {(isEnabled ? "disable" : "enable")} auto weekly cap for the character");

        if (isEnabled)
            ImGui.PopStyleColor();
    }

    internal static void DrawCharacterRelogIcon(string character)
    {
        var command = $"{AWC.CommandNameShort} relog {character}";

        ImGui.SameLine(0f, 4f);
        ImGuiEx.IconButton(FontAwesomeIcon.DoorOpen);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.SetClipboardText(command);
            Notify.Success("Link copied to clipboard");
        }
        else if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            ChatHelper.RunCommand(command);
        }

        ImGuiEx.Tooltip("Left click:   relog to this character\nRight click: copy relog command to clipboard");
    }

    internal static void DrawCharacterSettingsIcon(string character, CharacterOptions options)
    {
        ImGui.SameLine(0f, 4f);

        if (ImGuiEx.IconButton(FontAwesomeIcon.UserCog))
        {
            AWC.Instance.OpenCharacterOptionsUi(character);
        }

        ImGuiEx.Tooltip("Configure Character");
    }

    internal static void DrawCharacterDetails(string character, CharacterOptions options, int tomes, int weeklyLimit)
    {
        ImGui.SameLine(0f, 4f);

        var cursorPos = ImGui.GetCursorPos();
        ImGui.ProgressBar(0, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()), "");
        ImGui.SameLine();

        cursorPos.X += 8;
        ImGui.SetCursorPos(cursorPos);

        var characterText = character;
        if (options.PreferredJob != PlayerJob.None)
            characterText += $"  ({options.PreferredJob.GetName()})";

        ImGui.TextWrapped(characterText);

        if (options.HasOverrideSettingsEnabled())
        {
            ImGui.SameLine();
            ImGuiEx.IconWithText(ColorUtils.HexToVector(0x9B, 0x9B, 0xE9, 0.65f), FontAwesomeIcon.Flask, "");
        }

        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 64 + ImGui.GetStyle().ItemSpacing.X);

        ImGui.TextUnformatted($"{tomes}/{weeklyLimit}");
    }
}
