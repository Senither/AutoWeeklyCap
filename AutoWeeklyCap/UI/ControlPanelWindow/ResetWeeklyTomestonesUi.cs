using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class ResetWeeklyTomestonesUi
{
    public static void Draw()
    {
        Card.DrawWarning("Reset Weekly Tomestones", () =>
        {
            ImGui.TextWrapped(
                "The tomestones will reset automatically during the weekly reset, however, " +
                "if you want to reset the tomes manually you can use the button below."
            );

            ImGui.Spacing();
            ImGui.Spacing();

            ActionButton.Draw(
                "Reset Weekly Tomestones",
                "Hold down CTRL to reset your weekly tomestones",
                () => AWC.Config.CollectedTomes.Clear()
            );
        }, collapsible: false);
    }
}
