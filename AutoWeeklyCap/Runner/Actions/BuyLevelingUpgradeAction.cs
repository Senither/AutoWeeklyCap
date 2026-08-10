using AutoWeeklyCap.Contracts.Runner;
using AutoWeeklyCap.Runner.Actions.LevelingGear;

namespace AutoWeeklyCap.Runner.Actions;

public class BuyLevelingUpgradeAction : BaseAction
{
    protected override string Name => nameof(BuyLevelingUpgradeAction);
    protected override string[] AddonsToClose { get; } = ["SelectIconString", "SelectString", "Shop", "SelectYesno"];

    private readonly List<ExpansionGear> _expansionGears =
    [
        new Dawntrail(),
        new Endwalker(),
        new Shadowbringers(),
        new Stormblood(),
        new Heavensward(),
    ];

    protected override bool Run(params object[] args)
    {
        if (!LifestreamIPC.IsEnabled || !VNavMeshIPC.IsEnabled || !StylistIPC.IsEnabled) {
            return false;
        }

        if (AWC.Config.LevelJobs.MinimumGilThreshold >= CurrencyHelper.GetGil()) {
            return false;
        }

        if (InventoryHelper.GetEmptySlotsInBag() < 1) {
            LogInfo($"Stopping {Name}, reason: no items slot left");
            return true;
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
        if (expansion.IsAboveItemLevelThreshold(itemLevel)) {
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
