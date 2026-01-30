namespace AutoWeeklyCap.Commands;

public class StartCommand : ICommand
{
    public string[] Triggers { get; } = ["start", "s"];
    public string Description => "Start the runner, or resume if it's being stopped gracefully";

    public void Run(string[] args)
    {
        if (AWC.Runner.IsRunning())
        {
            if (AWC.Runner.IsStopping())
            {
                AWC.Runner.Resume();
            }
        }
        else
        {
            if (AWC.IsRequiredPluginsEnabled())
            {
                AWC.Runner.Start();
            }
        }
    }
}
