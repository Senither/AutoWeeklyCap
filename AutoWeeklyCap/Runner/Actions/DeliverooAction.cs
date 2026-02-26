using Dalamud.Game.Text.SeStringHandling;

namespace AutoWeeklyCap.Runner.Actions;

public class DeliverooAction : BaseAction
{
    protected override string Name => nameof(DeliverooAction);
    protected override string[] AddonsToClose { get; } = ["SelectYesno", "GrandCompanySupplyList", "GrandCompanyExchange", "SelectString"];

    private const int LongTaskTimeout = 450_000; // 7½ minute

    protected override bool Run(params object[] args)
    {
        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled || !DeliverooIPC.IsEnabled)
            return false;

        if (DeliverooIPC.IsTurnInRunning())
            DeliverooIPC.StopTurnIn();

        ActionInstance.LeaveGrandCompanyInn.Invoke();

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.PriorityWorld, "GC delivery");

        Enqueue(() =>
        {
            if (EzThrottler.Throttle("NavigatingToGcTerritory", 500))
                return false;

            if (Player.Territory.RowId == GrandCompanyHelper.TerritoryId)
                return true;

            if (LifestreamIPC.IsBusy())
                return false;

            LifestreamIPC.ExecuteCommand(GrandCompanyHelper.AetheriteName);

            return true;
        }, "start moving to gc territory");

        Enqueue(() =>
        {
            if (EzThrottler.Throttle("NavigatingToGcTerritory", 500))
                return false;

            return Player.Territory.RowId == GrandCompanyHelper.TerritoryId && PlayerHelper.IsReady;
        }, "waiting for player to be in gc territory");

        Enqueue(
            () => MovementHelper.MoveTo(GrandCompanyHelper.TurnInLocation),
            "start moving to gc NPC location",
            LongTaskTimeout
        );

        Enqueue(() =>
        {
            if (DeliverooIPC.IsTurnInRunning())
                return true;

            if (EzThrottler.Throttle("StartingDeliverooTurnIn", 500))
                DeliverooIPC.StartTurnIn();

            return false;
        }, "starting deliveroo turn in");

        Enqueue(() => !DeliverooIPC.IsTurnInRunning(), "waiting for deliveroo turn in to finish", LongTaskTimeout);

        return true;
    }
}
