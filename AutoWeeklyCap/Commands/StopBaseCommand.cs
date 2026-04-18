namespace AutoWeeklyCap.Commands;

public class StopBaseCommand : BaseCommand
{
    public override string[] Triggers { get; } = ["stop", "end"];
    public override string Description => "Stops the runner, if it's being stopped gracefully it will finish the duty before completely stopping";

    public override void Run(string[] args)
    {
        if (AWC.Runner.IsRunning() && !AWC.Runner.IsStopping()) {
            AWC.Runner.Stop();
        }
    }
}
