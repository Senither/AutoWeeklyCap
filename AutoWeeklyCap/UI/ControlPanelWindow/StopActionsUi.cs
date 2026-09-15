using AutoWeeklyCap.UI.ControlPanelWindow.Sections.StopActionsUi;
using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class StopActionsUi
{
    public static void Draw()
    {
        ConfigOverridesStatus.Draw(() =>
        {
            Card.Draw("Stop Actions", StopActionSelectorSection.Draw, collapsible: false);

            List<Action> elements = GetOptionsDrawElements();
            if (elements.Count == 0) {
                ImGui.Spacing();
                ImGui.Spacing();

                Disabled.Draw(() => ImGui.TextWrapped(
                    "Selecting certain options will show additional options here, allowing you to further customize the behaviour of the actions."
                ));

                return;
            }

            Card.Draw(AWC.Config.StopAction.GetName() + " Options", () =>
            {
                foreach (var element in elements) {
                    element.Invoke();
                }
            }, collapsible: false);
        });
    }

    private static List<Action> GetOptionsDrawElements()
    {
        return AWC.Config.StopAction switch
        {
            StopAction.SwitchCharacter or StopAction.StartUnlimitedRuns => [CharacterSwitchSection.Draw],
            StopAction.LevelJobs => [LevelJobElementsSection.Draw],
            _ => []
        };
    }
}
