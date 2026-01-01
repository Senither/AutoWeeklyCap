using AutoWeeklyCap.Helpers;

namespace AutoWeeklyCap.Commands;

public class StartCommand : ICommand
{
    public string[] Triggers { get; } = ["start", "s"];
    public string Description => "Start the runner, or resume if it's being stopped gracefully";

    public void Run(string[] args)
    {
        if (AutoWeeklyCap.Runner.IsRunning())
        {
            if (AutoWeeklyCap.Runner.IsStopping())
            {
                AutoWeeklyCap.Runner.Resume();
            }
        }
        else
        {
            if (Utils.IsRequiredPluginsEnabled())
            {
                AutoWeeklyCap.Runner.Start();
            }
        }
    }
}
