using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.StopActionsUi;

internal static class StopActionSelectorSection
{
    internal static void Draw()
    {
        ImGui.TextWrapped("Select what should happen when all characters have been tomestone capped.");

        Card.Separator();

        ImGui.Spacing();
        ImGui.Spacing();

        foreach (StopAction action in StopActionExtensions.GetOrderedStopActions()) {
            if (ImGui.RadioButton(action.GetName(), AWC.Config.StopAction == action)) {
                AWC.Config.StopAction = action;
            }

            Action? tooltip = action.GetTooltip();
            if (tooltip != null) {
                InformationTooltip.Draw(tooltip);
            }
        }
    }
}
