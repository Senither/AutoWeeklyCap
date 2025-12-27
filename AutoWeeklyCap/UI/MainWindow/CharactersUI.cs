using System.Numerics;
using AutoWeeklyCap.Config;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.UI.MainWindow;

internal static class CharactersUI
{
    internal static void Draw()
    {
        var charactersEnabled = 0;
        var totalTomesCollected = 0;
        var weeklyTomeLimit = InventoryManager.GetLimitedTomestoneWeeklyLimit();

        foreach (var character in AutoWeeklyCap.Config.Characters.Keys)
        {
            var option = AutoWeeklyCap.Config.Characters[character];
            var characterTomes = AutoWeeklyCap.Config.GetWeeklyTomes(character);
            totalTomesCollected += characterTomes;

            if (option.Enabled)
                charactersEnabled++;

            ImGui.PushID(character);

            DrawCharacterStatusIcon(character, option);
            DrawCharacterSettingsIcon(character, option);
            DrawCharacterDetails(character, characterTomes, weeklyTomeLimit);

            ImGui.PopID();
        }

        ImGui.Text($"Weekly tomestone cap is at {totalTomesCollected}/{weeklyTomeLimit * charactersEnabled}");

        if (ImGui.Button("Reset Weekly Tomes"))
        {
            AutoWeeklyCap.Config.CollectedTomes.Clear();
            AutoWeeklyCap.Config.Save();
        }
    }

    internal static void SaveCharacterConfigurationOption(string character, CharacterOptions options)
    {
        AutoWeeklyCap.Config.Characters[character] = options;
        AutoWeeklyCap.Config.Save();
    }

    internal static void DrawCharacterStatusIcon(string character, CharacterOptions option)
    {
        if (option.Enabled)
            ImGui.PushStyleColor(ImGuiCol.Button, 0xFF097000);

        if (ImGuiEx.IconButton(FontAwesomeIcon.Rocket))
        {
            option.Enabled = !option.Enabled;
            SaveCharacterConfigurationOption(character, option);
        }

        ImGuiEx.Tooltip($"Click to {(option.Enabled ? "disable" : "enable")} auto weekly cap for the character");

        if (option.Enabled)
            ImGui.PopStyleColor();
    }

    internal static void DrawCharacterSettingsIcon(string character, CharacterOptions options)
    {
        ImGui.SameLine();

        if (ImGuiEx.IconButton(FontAwesomeIcon.UserCog))
        {
            AutoWeeklyCap.Log.Debug($"Character settings for {character} will be rendered in the future...");
            // TODO: Open a tab or window where it's possible to configure the user settings   
        }

        ImGuiEx.Tooltip("Configure Character");
    }

    internal static void DrawCharacterDetails(string character, int tomes, int weeklyLimit)
    {
        ImGui.SameLine();

        var cursorPos = ImGui.GetCursorPos();
        ImGui.ProgressBar(0, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()), "");
        ImGui.SameLine();

        cursorPos.X += 8;
        ImGui.SetCursorPos(cursorPos);

        ImGui.TextWrapped(character);
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 64 + ImGui.GetStyle().ItemSpacing.X);

        ImGui.TextUnformatted($"{tomes}/{weeklyLimit}");
    }
}
