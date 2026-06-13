namespace AutoWeeklyCap.Commands;

public class StartBaseCommand : BaseCommand
{
    public override string[] Triggers { get; } = ["start", "s"];
    public override string Description => "Start the runner, or resume if it's being stopped gracefully";

    public override void Run(string[] args)
    {
        if (AWC.Runner.State.IsRunning()) {
            if (AWC.Runner.State.StoppingGracefully) {
                AWC.Runner.Resume();
            }
        } else {
            if (AWC.IsRequiredPluginsEnabled()) {
                AWC.Runner.Start();
            }
        }
    }
}
