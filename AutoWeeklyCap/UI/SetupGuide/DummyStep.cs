using ECommons.Logging;

namespace AutoWeeklyCap.UI.SetupGuide;

public class DummyStep : ISetupStep
{
    public string Title => "Dummy Step";

    public void Draw()
    {
        ImGui.Text("Welcome to AutoWeeklyCap (Dummy step)");
    }
}
