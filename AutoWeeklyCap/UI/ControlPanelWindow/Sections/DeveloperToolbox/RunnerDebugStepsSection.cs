using AutoWeeklyCap.UI.Helpers;

using ECommons.Configuration;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.DeveloperToolbox;

internal static class RunnerDebugStepsSection
{
    internal static void Draw()
    {
        ImGui.Text($"Current stage: {TitleManager.GetStatus() ?? "idle"}");

        DebugButton.Draw("Start", () => AWC.Runner.Start(), false);
        DebugButton.Draw("Start on Boot", () => AWC.Runner.AutoStartOnBoot());
        DebugButton.Draw("Stop", () => AWC.Runner.Stop());
        DebugButton.Draw("Resume", () => AWC.Runner.Resume());
        DebugButton.Draw("Abort", () => AWC.Runner.Abort());

        DebugButton.Draw("Export Runner State", () =>
        {
            ImGui.SetClipboardText(EzConfig.DefaultSerializationFactory.Serialize(AWC.Runner.State, true));
            Notify.Info("Runner state copied to clipboard");
        });
    }
}
