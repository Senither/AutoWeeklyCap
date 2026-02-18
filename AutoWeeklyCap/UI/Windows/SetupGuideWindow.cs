using AutoWeeklyCap.UI.Helpers;
using AutoWeeklyCap.UI.SetupGuide;
using Dalamud.Interface.Windowing;

namespace AutoWeeklyCap.UI.Windows;

public class SetupGuideWindow : Window
{
    private static int CurrentStep;

    private static readonly List<ISetupStep> Steps =
    [
        new WelcomeSetupStep(),
        new DummyStep()
    ];

    public SetupGuideWindow() : base("AWC Setup Guide##awc-setup-guide")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 400),
            MaximumSize = new Vector2(800, 1000),
        };
    }

    public override void Draw()
    {
        Steps[CurrentStep].Draw();
        ImGui.Dummy(new Vector2(28));
        DrawSetupGuideProgress();
    }

    private void DrawSetupGuideProgress()
    {
        ImGui.SetCursorPosY(ImGui.GetWindowSize().Y - 32);

        ImGui.BeginGroup();

        Disabled.Draw(CurrentStep <= 0, () =>
        {
            if (ImGui.Button("Previous", new Vector2(100, 0)))
                CurrentStep--;
        });

        ImGui.SameLine();
        ImGui.SetCursorPosX(120);
        ImGui.ProgressBar((float)CurrentStep / Steps.Count, new Vector2(ImGui.GetWindowSize().X - 240, 0), "");

        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowSize().X - 108);

        Disabled.Draw(CurrentStep >= Steps.Count, () =>
        {
            if (ImGui.Button(CurrentStep == Steps.Count - 1 ? "Close" : "Next", new Vector2(100, 0)))
            {
                if (CurrentStep == Steps.Count - 1)
                    CloseWindow();
                else
                    CurrentStep++;
            }
        });

        ImGui.EndGroup();

        ImGui.SetCursorPosY(ImGui.GetWindowSize().Y - 30);
        ImGuiEx.TextCentered(Steps[CurrentStep].Title);
    }

    private void CloseWindow()
    {
        IsOpen = false;
        CurrentStep = 0;

        AWC.Config.HasCompletedSetupGuide = true;
        AWC.Config.Save();
    }
}
