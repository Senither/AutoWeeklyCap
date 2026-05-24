using ECommons.Automation.NeoTaskManager;
using ECommons.UIHelpers.AddonMasterImplementations;

namespace AutoWeeklyCap.Runner.Actions;

public class DeliverooAction : BaseAction
{
    protected override string Name => nameof(DeliverooAction);
    protected override string[] AddonsToClose { get; } = ["SelectYesno", "GrandCompanySupplyList", "GrandCompanyExchange", "GrandCompanySupplyReward", "SelectString"];

    private const int LongTaskTimeout = 450_000; // 7½ minute
    private DateTime? _lastStuckAt = null;

    protected override bool Run(params object[] args)
    {
        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled || !DeliverooIPC.IsEnabled) {
            return false;
        }

        if (DeliverooIPC.IsTurnInRunning()) {
            DeliverooIPC.StopTurnIn();
        }

        ActionInstance.LeaveGrandCompanyInn.Invoke();
        LocationManager.Reset();

        _lastStuckAt = null;

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.PriorityWorld, "GC delivery");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("NavigatingToGcTerritory", 500)) {
                return false;
            }

            if (Player.Territory.RowId == GrandCompanyHelper.TerritoryId) {
                return true;
            }

            if (LifestreamIPC.IsBusy()) {
                return false;
            }

            LifestreamIPC.ExecuteCommand(GrandCompanyHelper.AetheriteName);

            return true;
        }, "start moving to gc territory");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("NavigatingToGcTerritory", 500)) {
                return false;
            }

            return Player.Territory.RowId == GrandCompanyHelper.TerritoryId && PlayerHelper.IsReady;
        }, "waiting for player to be in gc territory");

        Enqueue(
            () => MovementHelper.MoveTo(GrandCompanyHelper.TurnInLocation),
            "start moving to gc NPC location",
            LongTaskTimeout
        );

        Enqueue(() =>
        {
            if (DeliverooIPC.IsTurnInRunning()) {
                return true;
            }

            if (EzThrottler.Throttle("StartingDeliverooTurnIn", 500)) {
                DeliverooIPC.StartTurnIn();
            }

            return false;
        }, "starting deliveroo turn in");

        Enqueue(() =>
        {
            if (!DeliverooIPC.IsTurnInRunning()) {
                return true;
            }

            if (!EzThrottler.Throttle("CheckingDeliverooStatus", 500)) {
                return false;
            }

            try {
                return CheckDeliverooStatus();
            } catch (Exception) {
                return false;
            }
        }, "waiting for deliveroo turn in to finish", LongTaskTimeout);

        return true;
    }

    private unsafe bool CheckDeliverooStatus()
    {
        if (!AddonHelper.TryGetReadyAddon("SelectYesno", out var addon)) {
            _lastStuckAt = null;
            return false;
        }

        var selector = new AddonMaster.SelectYesno(addon);
        if (!selector.Text.Contains("materia")) {
            _lastStuckAt = null;
            return false;
        }

        _lastStuckAt ??= DateTime.UtcNow;

        return (DateTime.UtcNow - _lastStuckAt.Value).Seconds >= 5 && HandleStuckDeliveroo(selector);
    }

    private bool HandleStuckDeliveroo(AddonMaster.SelectYesno select)
    {
        switch (AWC.Config.DeliverooStuckAction) {
            case DeliverooStuckAction.StopDeliveroo:
                AWC.TaskManager.InsertMulti(
                    new TaskManagerTask(select.No, $"{Name}: selecting no in addon"),
                    new TaskManagerTask(DeliverooIPC.StopTurnIn, $"{Name}: stopping deliveroo"),
                    new TaskManagerTask(CloseAddons, $"{Name}: closing deliveroo addons")
                );
                break;

            case DeliverooStuckAction.HandInMateriaItem:
                select.Yes();
                return false;

            default:
                AWC.Log.Error($"{Name}: Hit argument out of range during stuck deliveroo handling");
                throw new ArgumentOutOfRangeException();
        }

        return true;
    }
}
