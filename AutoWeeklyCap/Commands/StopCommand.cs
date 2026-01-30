namespace AutoWeeklyCap.Commands;

public class StopCommand : ICommand
{
    public string[] Triggers { get; } = ["stop", "end"];

    public string Description => "Stops the runner, if it's being stopped gracefully it will finish the duty before completely stopping";

    public void Run(string[] args)
    {
        if (AWC.Runner.IsRunning() && !AWC.Runner.IsStopping())
            AWC.Runner.Stop();
    }
}
