using ECommons.ExcelServices;

namespace AutoWeeklyCap.Runner.Actions;

public class DeliverooAction : BaseAction
{
    protected override string Name => nameof(DeliverooAction);
    protected override string[] AddonsToClose { get; } = ["SelectYesno", "GrandCompanySupplyList", "GrandCompanyExchange", "SelectString"];

    private const int LongTaskTimeout = 450_000; // 7½ minute

    protected override bool Run()
    {
        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled || !DeliverooIPC.IsEnabled)
            return false;

        if (DeliverooIPC.IsTurnInRunning())
            DeliverooIPC.StopTurnIn();

        Enqueue(() =>
        {
            if (EzThrottler.Throttle("NavigatingToGcTerritory", 500))
                return false;

            if (Player.Territory.RowId == PlayerHelper.GetGrandCompanyTerritoryType())
                return true;

            if (LifestreamIPC.IsBusy())
                return false;

            LifestreamIPC.ExecuteCommand(PlayerHelper.GetGrandCompanyAetheriteName());

            return true;
        }, "start moving to gc territory");

        Enqueue(() =>
        {
            if (EzThrottler.Throttle("NavigatingToGcTerritory", 500))
                return false;

            return Player.Territory.RowId == PlayerHelper.GetGrandCompanyTerritoryType() && PlayerHelper.IsReady;
        }, "waiting for player to be in gc territory");

        Enqueue(
            () => MovementHelper.MoveTo(GrandCompanyTurnInLocation),
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

    private static Vector3 GrandCompanyTurnInLocation => PlayerHelper.GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => new Vector3(94.02527f, 40.275368f, 75.61174f),
        GrandCompany.TwinAdder => new Vector3(-67.994354f, -0.50152725f, -8.873131f),
        _ => new Vector3(-142.4761f, 4.0999994f, -106.80103f),
    };
}
