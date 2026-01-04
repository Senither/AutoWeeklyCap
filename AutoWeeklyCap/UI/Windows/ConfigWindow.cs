using System;
using System.Numerics;
using AutoWeeklyCap.UI.ConfigWindow;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Interface.Windowing;

namespace AutoWeeklyCap.UI.Windows;

public class ConfigWindow : Window, IDisposable
{
    public ConfigWindow() : base("Auto Weekly Tomestone Settings")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 125),
            MaximumSize = new Vector2(9999, 9999)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        var hasHiddenCharacters = false;
        foreach (var option in AutoWeeklyCap.Config.Characters.Values)
        {
            if (option.IsHidden())
            {
                hasHiddenCharacters = true;
            }
        }

        Card.Draw("Duty Options", DutyOptionsUi.Draw);

        if (hasHiddenCharacters)
            Card.Draw("Hidden Characters", HiddenCharactersUi.Draw);

        Card.Draw("Between Runs Options", BetweenRunOptionsUi.Draw);

        Card.Draw("Stop Actions", StopActionsUi.Draw);
        Card.DrawWarning("Manually reset Tomestones", ResetWeeklyTomestonesUi.Draw);
    }

    public override void OnClose()
    {
        AutoWeeklyCap.Config.Save();
    }
}
