namespace AutoWeeklyCap.Runner.Actions;

public class EquipGearUpgradeAction : BaseAction
{
    protected override string Name => nameof(EquipGearUpgradeAction);

    protected override bool Run(params object[] args)
    {
        if (!StylistIPC.IsEnabled) {
            return false;
        }

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.ArrowUp, "Equipping gear");

        Enqueue(() => PlayerHelper.IsReady, "wait for player");
        Enqueue(StylistIPC.UpdateCurrentGearsetAndEquip, "equipping gear upgrades");
        Enqueue(() => !StylistIPC.IsBusy(), "wait for stylist to equip gear");
        EnqueueDelay(500);

        return true;
    }
}
