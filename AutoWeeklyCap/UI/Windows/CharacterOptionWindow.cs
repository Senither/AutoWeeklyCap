using System;
using System.Numerics;
using AutoWeeklyCap.Config;
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

    public void ToggleForCharacterWithOptions(string character)
    {
        if (IsOpen && this.character == character)
        {
            OnClose();
            return;
        }

        this.character = character;
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
}
