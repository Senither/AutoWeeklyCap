namespace AutoWeeklyCap.UI.SetupGuide;

public interface ISetupStep
{
    string Title { get; }

    void Draw();
}
