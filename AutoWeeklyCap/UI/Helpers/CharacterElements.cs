using AutoWeeklyCap.Config;
using Dalamud.Interface;

namespace AutoWeeklyCap.UI.Helpers;

public static class CharacterElements
{
    internal static void DrawCharacterVisibilityIcon(string character, CharacterOptions option, bool sameLine = false)
    {
        if (sameLine)
            ImGui.SameLine(0f, 4f);

        var isHidden = option.Hidden;
        if (!isHidden)
            ImGui.PushStyleColor(ImGuiCol.Button, 0xFF097000);

        if (ImGuiEx.IconButton(isHidden ? FontAwesomeIcon.EyeSlash : FontAwesomeIcon.Eye))
        {
            option.Hidden = !isHidden;
            SaveCharacterConfigurationOption(character, option);
        }

        ImGuiEx.Tooltip($"Click to {(isHidden ? "show" : "hide")} this character");

        if (!isHidden)
            ImGui.PopStyleColor();
    }

    internal static void DrawCharacterStatusIcon(string character, CharacterOptions option, bool sameLine = false)
    {
        if (sameLine)
            ImGui.SameLine(0f, 4f);

        var isEnabled = option.IsEnabled();
        if (isEnabled)
            ImGui.PushStyleColor(ImGuiCol.Button, 0xFF097000);

        if (ImGuiEx.IconButton(FontAwesomeIcon.Rocket))
        {
            option.Enabled = !isEnabled;
            SaveCharacterConfigurationOption(character, option);
        }

        ImGuiEx.Tooltip(option.IsHidden()
                            ? "Character is automatically disabled because it's hidden"
                            : $"Click to {(isEnabled ? "disable" : "enable")} this character"
        );

        if (isEnabled)
            ImGui.PopStyleColor();
    }

    internal static void DrawCharacterRelogIcon(string character, bool sameLine = false)
    {
        var command = $"{AWC.CommandNameShort} relog {character}";

        if (sameLine)
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

    internal static void DrawCharacterSettingsIcon(string character, bool sameLine = false)
    {
        if (sameLine)
            ImGui.SameLine(0f, 4f);

        if (ImGuiEx.IconButton(FontAwesomeIcon.UserCog))
        {
            AWC.Instance.OpenCharacterOptionsUi(character);
        }

        ImGuiEx.Tooltip("Configure Character");
    }

    private static void SaveCharacterConfigurationOption(string character, CharacterOptions options)
    {
        AWC.Config.Characters[character] = options;
        AWC.Config.Save();
    }
}
