using AutoWeeklyCap.IPC.AutoDuty;
using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.DeveloperToolbox;

internal static class AutoDutyConfigOverridesSection
{
    internal static void Draw()
    {
        DebugButton.Draw("Apply Overrides", AutoDutyProfile.Apply, false);
        DebugButton.Draw("Pop Overrides", AutoDutyProfile.Pop);
    }
}
