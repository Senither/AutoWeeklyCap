using AutoWeeklyCap.UI.ControlPanelWindow.Sections.RunnerPrerequisitesUi;
using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class RunnerPrerequisitesUi
{
    public static void Draw()
    {
        ConfigOverridesStatus.Draw(() =>
        {
            Card.Draw("Duty Options###runner-duty-options", DutyOptionsSection.Draw, defaultOpen: true);
            Card.Draw("Before Runs###runner-before-general", BeforeRunOptionsSection.Draw, defaultOpen: true);
            Card.Draw("Between Runs###runner-between-general", BetweenRunsOptionsSection.Draw, defaultOpen: true);

            ImGui.TextWrapped("Select how third-party plugins should be integrated into the runner.");
            ImGui.Spacing();

            Card.Draw("Auto Retainer###runner-prereq-auto-retainer", AutoRetainerSection.Draw);
            Card.Draw("Deliveroo###runner-prereq-deliveroo", DeliverooSection.Draw);
            Card.Draw("Notification Master###runner-prereq-notification-master", NotificationMasterSection.Draw);
        });
    }
}
