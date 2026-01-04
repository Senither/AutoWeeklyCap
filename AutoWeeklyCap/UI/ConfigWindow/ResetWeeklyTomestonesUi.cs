using AutoWeeklyCap.UI.Helpers;
using Dalamud.Bindings.ImGui;

namespace AutoWeeklyCap.UI.ConfigWindow;

public static class ResetWeeklyTomestonesUi
{
    public static void Draw()
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
            () => AutoWeeklyCap.Config.CollectedTomes.Clear()
        );
    }
}
