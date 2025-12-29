using System;
using System.Numerics;
using AutoWeeklyCap.Config;
using AutoWeeklyCap.Runner;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

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

        Card.Draw("Character visibility", () => DrawCharacterVisibility(options));
        Card.Draw("Character Preferences", () => DrawCharacterPreferences(options));
        Card.DrawDanger("Remove Character", DrawCharacterRemoval);
    }

    private void DrawCharacterVisibility(CharacterOptions options)
    {
        var hidden = options.IsHidden();
        if (ImGui.Checkbox("Hide Character###character-visibility", ref hidden))
        {
            options.Hidden = hidden;
        }

        InformationTooltip.Draw("Hides the character from the list, and disables it for tome runs");
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
            "Automatically swaps to your preferred job before starting AutoDuty, if none is selected your job will not be changed");
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

                AutoWeeklyCap.Config.Characters.Remove(character);
                OnClose();
            }
        );
    }
}
