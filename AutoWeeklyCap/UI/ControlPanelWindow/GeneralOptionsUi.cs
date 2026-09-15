using AutoWeeklyCap.UI.ControlPanelWindow.Sections.GeneralOptions;
using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class GeneralOptionsUi
{
    public static void Draw()
    {
        ConfigOverridesStatus.Draw(() =>
        {
            Card.Draw("General Options", GeneralOptionsSection.Draw, defaultOpen: true);
            Card.Draw("UI Elements, Windows & Sounds", UiElementsWindowsAndSoundsSection.Draw, defaultOpen: true);
            Card.Draw("Status Icon", StatusIconSection.Draw, defaultOpen: true);
            Card.Draw("Network Options", NetworkOptionsSection.Draw, defaultOpen: true);
            Card.DrawWarning("Reset Weekly Tomestones", ResetWeeklyTomestonesSection.Draw);
        });
    }
}
