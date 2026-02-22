namespace AutoWeeklyCap.Runner.Actions;

public class ReturnToHomeworldAction : BaseAction
{
    protected override string Name => nameof(ReturnToHomeworldAction);
    protected override string[] AddonsToClose { get; } = ["ShopExchangeCurrency", "SelectIconString", "SelectYesno", "SelectString", "DrawStory"];

    private const int LongTaskTimeout = 600_000; // 10 minute

    protected override bool Run(params object[] args)
    {
        if (!Player.Available || !LifestreamIPC.IsEnabled)
            return false;

        if (Player.CurrentWorld.RowId == Player.HomeWorld.RowId)
            return false;

        var homeworld = Player.HomeWorld.ValueNullable?.Name;
        if (homeworld == null)
            return false;

        Enqueue(() =>
        {
            LifestreamIPC.ExecuteCommand(homeworld.ToString() ?? string.Empty);
            return true;
        }, "return to home world");

        Enqueue(
            () => !LifestreamIPC.IsBusy(),
            "wait for return to home world",
            LongTaskTimeout
        );

        Enqueue(() => PlayerHelper.IsReady, "wait for player to be logged in");
        EnqueueDelay(2500);

        return true;
    }
}
