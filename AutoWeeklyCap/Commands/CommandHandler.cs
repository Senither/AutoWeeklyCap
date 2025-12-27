namespace AutoWeeklyCap.Commands;

public static class CommandHandler
{
    public static void HandleCommand(string[] args)
    {
        AutoWeeklyCap.Instance.ToggleMainUi();

        // TODO: Add propper commands using the arguments    
    }
}
