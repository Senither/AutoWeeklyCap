using AutoWeeklyCap.Config;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;

namespace AutoWeeklyCap.UI.Windows;

public class CharacterOptionWindow : Window
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

        AWC.Config.Save();
    }

    public override void PreDraw()
    {
        WindowName = $"{character} Configuration###character-options-window";
    }

    public override void Draw()
    {
        if (character == null)
            return;

        var options = AWC.Config.GetOrRegisterCharacterOptions(character);

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

        Disabled.Draw(options.Position == AWC.Config.Characters.Count - 1, () =>
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

        AWC.Config.NormalizeCharacterPositions();

        var sortedCharacters = AWC.Config.GetSortedCharacters();
        var currentIndex = sortedCharacters.IndexOf(character);
        if (currentIndex == -1)
            return;

        var targetIndex = currentIndex + direction;
        if (targetIndex < 0 || targetIndex >= sortedCharacters.Count)
            return;

        var otherCharacter = sortedCharacters[targetIndex];
        var currentOptions = AWC.Config.GetOrRegisterCharacterOptions(character);
        var otherOptions = AWC.Config.GetOrRegisterCharacterOptions(otherCharacter);

        (currentOptions.Position, otherOptions.Position) = (otherOptions.Position, currentOptions.Position);

        AWC.Config.NormalizeCharacterPositions();
    }

    private void DrawCharacterPreferences(CharacterOptions options)
    {
        ImGui.Text("Preferred job");
        if (ImGui.BeginCombo($"###selected-duty-job", options.PreferredJob.GetName()))
        {
            foreach (var job in PlayerJobExtensions.GetSelectableCombatJobs())
            {
                if (ImGui.Selectable(job.GetName(), options.PreferredJob == job))
                {
                    options.PreferredJob = job;
                }
            }

            ImGui.EndCombo();
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Automatically swaps to your preferred job before starting AutoDuty");
            ImGui.Text("If none is selected your job will not be changed");
        });

        ImGui.Text("Preferred items to buy");
        if (ImGui.BeginCombo($"###selected-item-name", options.PreferredTomestoneItemName ?? "Use default"))
        {
            if (ImGui.Selectable("Use default", options.PreferredTomestoneItemName == null))
                options.PreferredTomestoneItemName = null;
            if (ImGui.Selectable("--------------------------------"))
                options.PreferredTomestoneItemName = null;

            foreach (var item in TomestoneItemHelper.GetTomestoneItems())
            {
                if (ImGui.Selectable(item.Name, options.PreferredTomestoneItemName == item.Name))
                    options.PreferredTomestoneItemName = item.Name;
            }

            ImGui.EndCombo();
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Buys the selected item for this characters with tomestones instead");
            ImGui.Text("of the item selected for all characters in the plugin settings");
            ImGui.Text("Overrides the \"Runner Options\" > \"Item to buy\" option");
        });
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

                AWC.Config.Characters.Remove(removedCharacter);
                AWC.Config.CollectedTomes.Remove(removedCharacter);
                AWC.Config.NormalizeCharacterPositions();
                OnClose();
            }
        );
    }
}
