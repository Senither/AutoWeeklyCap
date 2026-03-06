namespace AutoWeeklyCap.Runner.Actions;

public class LeaveGrandCompanyInnAction : BaseAction
{
    protected override string Name => nameof(LeaveGrandCompanyInnAction);

    protected override string[] AddonsToClose { get; } = ["MaterializeDialog", "Materialize", "SelectYesno", "SelectString"];

    protected override bool Run(params object[] args)
    {
        if (!VNavMeshIPC.IsEnabled)
            return false;

        if (Player.Territory.RowId != GrandCompanyHelper.InnTerritoryId)
            return false;

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.WatchingCutscene, "Leaving GC inn");

        Enqueue(() =>
        {
            if (EzThrottler.Throttle("MoveToGCDoorPosition", 250))
                return false;

            var gameObject = ObjectHelper.FindGameObject(GrandCompanyHelper.InnDoorId, Player.Position);

            return gameObject != null && MovementHelper.MoveTo(gameObject.Position, 4.75f);
        }, "start moving to GC door");

        Enqueue(() =>
        {
            if (EzThrottler.Throttle("LeavingGCInstance", 250))
                return false;

            if (Player.Territory.RowId != GrandCompanyHelper.InnTerritoryId)
                return true;

            var gameObject = ObjectHelper.FindGameObject(GrandCompanyHelper.InnDoorId, Player.Position);
            if (gameObject == null)
                return false;

            unsafe {
                if (AddonHelper.TryGetReadyAddon("SelectYesno", out _))
                    AddonHelper.ClickSelectYesno();
                else if (PlayerHelper.IsReady)
                    ObjectHelper.InteractWithObject(gameObject);
            }

            return false;
        }, "leave GC instance");

        Enqueue(() => PlayerHelper.IsReady, "wait for player to be ready");
        EnqueueDelay(1500);

        return true;
    }
}
