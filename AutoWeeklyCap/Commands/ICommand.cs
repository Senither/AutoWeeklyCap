namespace AutoWeeklyCap.Commands;

public interface ICommand
{
    string[] Triggers { get; }
    string Description { get; }

    void Run(string[] args);
}
