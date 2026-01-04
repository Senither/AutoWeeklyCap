using System;
using System.Numerics;
using AutoWeeklyCap.Config;
using AutoWeeklyCap.Runner;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;

namespace AutoWeeklyCap.UI.Windows;

public class CharacterOptionWindow : Window, IDisposable
{
    private string? character = null;

    public CharacterOptionWindow() : base("Character Options##character-options-window")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 125),
            MaximumSize = new Vector2(9999, 9999)
        };
    }

    public void Dispose() { }

    public void ToggleForCharacterWithOptions(string characterAndWorld)
    {
        if (IsOpen && character == characterAndWorld)
        {
            OnClose();
            return;
        }

        character = characterAndWorld;
        IsOpen = true;
    }

    public override void OnClose()
    {
        character = null;
        IsOpen = false;

        AutoWeeklyCap.Config.Save();
    }

    public override void PreDraw()
    {
        WindowName = $"{character} Configuration###character-options-window";
    }

    public override void Draw()
    {
        if (character == null)
            return;

        var options = AutoWeeklyCap.Config.GetOrRegisterCharacterOptions(character);

        Card.Draw("Character visibility", () => DrawCharacterVisibility(options), collapsible: false);
        Card.Draw("Character Preferences", () => DrawCharacterPreferences(options), collapsible: false);
        Card.DrawDanger("Remove Character", DrawCharacterRemoval, collapsible: false);
    }

    private void DrawCharacterVisibility(CharacterOptions options)
    {
        var hidden = options.IsHidden();
        if (ImGui.Checkbox("Hide Character###character-visibility", ref hidden))
        {
            options.Hidden = hidden;
        }

        InformationTooltip.Draw("Hides the character from the list, and disables it for tomestone runs");

        ImGui.Spacing();
        ImGui.Spacing();

        ImGui.Text("Character Position");

        Disabled.Draw(options.Position == 0, () =>
        {
            if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowUp))
                MoveCharacterPosition(-1);

            ImGuiEx.Tooltip("Move character up");
        });

        ImGui.SameLine(0f, 4f);

        Disabled.Draw(options.Position == AutoWeeklyCap.Config.Characters.Count - 1, () =>
        {
            if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowDown))
                MoveCharacterPosition(1);

            ImGuiEx.Tooltip("Move character down");
        });

        InformationTooltip.Draw(
            "The order of the characters are used when checking tomestones in the runner, so the\n"
            + "first character with missing tomestones is selected searching from top to bottom."
        );
    }

    private void MoveCharacterPosition(int direction)
    {
        if (character == null)
            return;

        AutoWeeklyCap.Config.NormalizeCharacterPositions();

        var sortedCharacters = AutoWeeklyCap.Config.GetSortedCharacters();
        var currentIndex = sortedCharacters.IndexOf(character);
        if (currentIndex == -1)
            return;

        var targetIndex = currentIndex + direction;
        if (targetIndex < 0 || targetIndex >= sortedCharacters.Count)
            return;

        var otherCharacter = sortedCharacters[targetIndex];
        var currentOptions = AutoWeeklyCap.Config.GetOrRegisterCharacterOptions(character);
        var otherOptions = AutoWeeklyCap.Config.GetOrRegisterCharacterOptions(otherCharacter);

        (currentOptions.Position, otherOptions.Position) = (otherOptions.Position, currentOptions.Position);

        AutoWeeklyCap.Config.NormalizeCharacterPositions();
    }

    private void DrawCharacterPreferences(CharacterOptions options)
    {
        ImGui.TextWrapped("Preferred job");

        if (ImGui.BeginCombo($"###selected-duty", options.PreferredJob.GetName()))
        {
            foreach (var job in PlayerJobExtensions.GetValues())
            {
                if (ImGui.Selectable(job.GetName(), options.PreferredJob == job))
                {
                    options.PreferredJob = job;
                }
            }

            ImGui.EndCombo();
        }

        InformationTooltip.Draw(
            "Automatically swaps to your preferred job before starting AutoDuty\n" +
            "If none is selected your job will not be changed"
        );
    }

    private void DrawCharacterRemoval()
    {
        ImGui.TextWrapped("Removing the character will delete any information about the character in the plugin.");
        ImGui.TextWrapped("You'll need to login to the character again after it's removed to re-add it back.");

        ImGui.Spacing();
        ImGui.Spacing();

        ActionButton.Draw(
            "Remove Character",
            "Hold down CTRL to remove " + character,
            () =>
            {
                if (character == null)
                    return;

                var removedCharacter = character;

                AutoWeeklyCap.Config.Characters.Remove(removedCharacter);
                AutoWeeklyCap.Config.CollectedTomes.Remove(removedCharacter);
                AutoWeeklyCap.Config.NormalizeCharacterPositions();
                OnClose();
            }
        );
    }
}
