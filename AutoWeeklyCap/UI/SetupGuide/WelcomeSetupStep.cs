namespace AutoWeeklyCap.UI.SetupGuide;

public class WelcomeSetupStep : ISetupStep
{
    public string Title => "Welcome Step";

    public void Draw()
    {
        ImGui.Text("Welcome to AutoWeeklyCap");
    }
}
