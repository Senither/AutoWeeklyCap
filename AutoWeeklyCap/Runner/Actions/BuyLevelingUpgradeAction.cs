using AutoWeeklyCap.Runner.Actions.LevelingGear;

namespace AutoWeeklyCap.Runner.Actions;

public class BuyLevelingUpgradeAction : BaseAction
{
    protected override string Name => nameof(BuyLevelingUpgradeAction);
    protected override string[] AddonsToClose { get; } = ["SelectIconString", "SelectString", "Shop", "SelectYesno"];

    private readonly List<ExpansionGear> _expansionGears =
    [
        new Heavensward(),
    ];

    protected override bool Run(params object[] args)
    {
        if (!LifestreamIPC.IsEnabled || !VNavMeshIPC.IsEnabled || !StylistIPC.IsEnabled) {
            return false;
        }

        var job = PlayerHelper.GetCurrentJob();
        var level = PlayerHelper.GetJobLevel(job);

        if (Constants.CurrentMaxLevel == level) {
            return false;
        }

        var expansion = GetExpansionGear(level);
        if (expansion == null) {
            return false;
        }

        var itemLevel = InventoryHelper.GetCurrentItemLevel();
        if (expansion.ItemLevelThreshold <= itemLevel) {
            return false;
        }

        LocationManager.Reset();

        using (TitleManager.RegisterTitle(BitmapFontIcon.LevelSync, "Buying leveling gear")) {
            expansion.EnqueueSequence(job);
        }

        ActionInstance.EquipGearUpgrade.Invoke();

        return true;
    }

    private ExpansionGear? GetExpansionGear(int level)
    {
        return _expansionGears.FirstOrDefault(expansionGear => expansionGear.MinimumLevel <= level);
    }
}
