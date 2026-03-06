using AutoWeeklyCap.UI.ConfigWindow;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface.Windowing;

namespace AutoWeeklyCap.UI.Windows;

public class ConfigWindow : Window
{
    public ConfigWindow() : base("Auto Weekly Tomestone Settings")
    {
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(525, 350), MaximumSize = new Vector2(9999, 9999) };
    }

    public override void Draw()
    {
        Card.Draw("General Options", GeneralOptionsUi.Draw);
        Card.Draw("Duty Options", DutyOptionsUi.Draw);
        Card.Draw("Characters", CharactersOptionUi.Draw);
        Card.Draw("Runner Options", RunnerPrerequisitesUi.Draw);
        Card.Draw("Stop Actions", StopActionsUi.Draw);
        Card.DrawWarning("Manually reset Tomestones", ResetWeeklyTomestonesUi.Draw);
    }

    public override void OnClose()
    {
        AWC.Config.Save();
    }
}
