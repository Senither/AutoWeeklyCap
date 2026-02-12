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

        Enqueue(() =>
        {
            LifestreamIPC.ExecuteCommand("");
            return true;
        }, "return to home world");

        Enqueue(
            () => !LifestreamIPC.IsBusy(),
            "wait for return to home world",
            LongTaskTimeout
        );

        EnqueueDelay(500);

        return true;
    }
}
