namespace AutoWeeklyCap.Runner.Actions;

public class EnterGrandCompanyInnAction : BaseAction
{
    protected override string Name => nameof(EnterGrandCompanyInnAction);

    protected override string[] AddonsToClose { get; } = ["SelectYesno", "SelectString", "Talk"];

    private const int LongTaskTimeout = 120_000;

    protected override bool Run(params object[] args)
    {
        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled)
            return false;

        if (Player.Territory.RowId == GrandCompanyHelper.InnTerritoryId)
            return false;

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.WatchingCutscene, "Entering GC inn");

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
            () => MovementHelper.MoveTo(GrandCompanyHelper.InnVendorLocation),
            "start moving to npc location",
            LongTaskTimeout
        );

        Enqueue(() =>
        {
            if (EzThrottler.Throttle("EnteringGCInstance", 250))
                return false;

            if (Player.Territory.RowId == GrandCompanyHelper.InnTerritoryId)
                return true;

            var gameObject = ObjectHelper.FindGameObject(GrandCompanyHelper.InnVendorId, GrandCompanyHelper.InnVendorLocation);
            if (gameObject == null)
                return false;

            unsafe
            {
                if (AddonHelper.TryGetReadyAddon("Talk", out _))
                    AddonHelper.ClickTalk();
                else if (AddonHelper.TryGetReadyAddon("SelectString", out _))
                    AddonHelper.ClickSelectString(0);
                else if (PlayerHelper.IsReady)
                    ObjectHelper.InteractWithObject(gameObject);
            }

            return false;
        }, "entering GC instance");

        Enqueue(() => PlayerHelper.IsReady, "wait for player to be ready");
        EnqueueDelay(1500);

        return true;
    }
}
