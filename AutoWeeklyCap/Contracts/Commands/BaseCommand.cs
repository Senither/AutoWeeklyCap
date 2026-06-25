namespace AutoWeeklyCap.Contracts.Commands;

public abstract class BaseCommand
{
    public abstract string[] Triggers { get; }
    public abstract string Description { get; }
    public bool Hidden { get; protected init; } = false;

    public abstract void Run(string[] args);
}
