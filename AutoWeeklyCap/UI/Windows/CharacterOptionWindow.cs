using AutoWeeklyCap.Config;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface.Windowing;

namespace AutoWeeklyCap.UI.Windows;

public class CharacterOptionWindow : Window
{
    private string? _character = null;

    public CharacterOptionWindow() : base("Character Options##character-options-window")
    {
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(300, 125), MaximumSize = new Vector2(9999, 9999) };
    }

    public void ToggleForCharacterWithOptions(string characterAndWorld)
    {
        if (IsOpen && _character == characterAndWorld) {
            OnClose();
            return;
        }

        _character = characterAndWorld;
        IsOpen = true;
    }

    public override void OnClose()
    {
        _character = null;
        IsOpen = false;

        Configuration.Save();
    }

    public override void PreDraw()
    {
        WindowName = $"{_character} Configuration###character-options-window";
    }

    public override void Draw()
    {
        if (_character == null) {
            return;
        }

        var options = AWC.Config.GetOrRegisterCharacterOptions(_character);
        if (options == null) {
            return;
        }

        using (Theme.Push()) {
            Card.Draw("Character visibility", () => DrawCharacterVisibility(options), false);
            Card.Draw("Character Preferences", () => DrawCharacterPreferences(options), false);
            Card.DrawDanger("Remove Character", DrawCharacterRemoval, false);
        }
    }

    private void DrawCharacterVisibility(CharacterOptions options)
    {
        var hidden = options.IsHidden();
        if (ImGui.Checkbox("Hide Character###character-visibility", ref hidden)) {
            options.Hidden = hidden;
        }

        InformationTooltip.Draw("Hides the character from the list, and disables it for tomestone runs");

        ImGui.Spacing();
        ImGui.Spacing();

        ImGui.Text("Character Position");

        CharacterElements.DrawCharacterPositionIcons(_character ?? string.Empty, options);

        InformationTooltip.Draw(
            "The order of the characters are used when checking tomestones in the runner, so the\n"
            + "first character with missing tomestones is selected searching from top to bottom."
        );
    }

    private void DrawCharacterPreferences(CharacterOptions options)
    {
        ImGui.Text("Preferred job");
        if (ImGui.BeginCombo($"###selected-duty-job", options.PreferredJob.GetName())) {
            foreach (var job in PlayerJobExtensions.GetSelectableCombatJobs()) {
                if (ImGui.Selectable(job.GetName(), options.PreferredJob == job)) {
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
        if (ImGui.BeginCombo($"###selected-item-name", options.PreferredTomestoneItemName ?? "Use default")) {
            if (ImGui.Selectable("Use default", options.PreferredTomestoneItemName == null)) {
                options.PreferredTomestoneItemName = null;
            }

            if (ImGui.Selectable("--------------------------------")) {
                options.PreferredTomestoneItemName = null;
            }

            foreach (var item in TomestoneItemHelper.GetTomestoneItems()) {
                if (InventoryHelper.TryGetSheetItemFromItemId(item.ItemId, out var itemObj)) {
                    ItemIcon.Draw(itemObj.Icon);
                }

                if (ImGui.Selectable(item.Name, options.PreferredTomestoneItemName == item.Name)) {
                    options.PreferredTomestoneItemName = item.Name;
                }
            }

            ImGui.EndCombo();
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Buys the selected item for this characters with tomestones instead");
            ImGui.Text("of the item selected for all characters in the plugin settings");
            ImGui.Text("Overrides the \"Runner Options\" > \"Item to buy\" option");
        });

        ImGui.Text("Preferred safezone");

        if (ImGui.BeginCombo($"###selected-safezone", options.PreferredSafezone?.GetName() ?? "Use default")) {
            if (ImGui.Selectable("Use default", options.PreferredSafezone == null)) {
                options.PreferredSafezone = null;
            }

            if (ImGui.Selectable("--------------------------------")) {
                options.PreferredSafezone = null;
            }

            foreach (var item in Enum.GetValues<Safezone>()) {
                if (ImGui.Selectable(item.GetName(), options.PreferredSafezone == item)) {
                    options.PreferredSafezone = item;
                }
            }

            ImGui.EndCombo();
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Overrides the safezone used for this character, when the runner attempts");
            ImGui.Text("to go to the safezone your selected safezone will be checked first,");
            ImGui.Text("if it fails it will fallback to the normal safezone order.");
        });
    }

    private void DrawCharacterRemoval()
    {
        ImGui.TextWrapped("Removing the character will delete any information about the character in the plugin.");
        ImGui.TextWrapped("You'll need to login to the character again after it's removed to re-add it back.");

        ImGui.Spacing();
        ImGui.Spacing();

        ActionButton.Draw("Remove Character", "Hold down CTRL to remove " + _character, () =>
            {
                if (_character == null) {
                    return;
                }

                var removedCharacter = _character;

                AWC.Config.Characters.Remove(removedCharacter);
                AWC.Config.CollectedTomes.Remove(removedCharacter);
                AWC.Config.NormalizeCharacterPositions();
                OnClose();
            }
        );
    }
}
