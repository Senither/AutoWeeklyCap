using AutoWeeklyCap.Contracts.Runner;
using AutoWeeklyCap.Runner.Actions;
using AutoWeeklyCap.Runner.Actions.Safezones;

namespace AutoWeeklyCap.Runner;

public static class ActionInstance
{
    public static readonly ExtractNamedTasks Extract = new();
    public static readonly AutoSpendTomestoneNamedTasks SpendTomestone = new();
    public static readonly SelfRepairNamedTasks SelfRepair = new();
    public static readonly NpcRepairNamedTasks NpcRepair = new();
    public static readonly ReturnToHomeworldNamedTasks Homeworld = new();
    public static readonly DeliverooNamedTasks Deliveroo = new();
    public static readonly NotificationNamedTasks Notification = new();
    public static readonly EquipGearUpgradeNamedTasks EquipGearUpgrade = new();
    public static readonly BuyLevelingUpgradeNamedTasks BuyLevelingUpgrade = new();

    // Safe-zone instances
    public static readonly SafezoneNamedTasks Safezone = new();

    public static readonly EnterGrandCompanyInnNamedTasks EnterGrandCompanyInn = new();
    public static readonly LeaveGrandCompanyInnNamedTasks LeaveGrandCompanyInn = new();
    public static readonly EnterApartmentNamedTasks EnterApartmentNamedTasks = new();
    public static readonly EnterPrivateHouseNamedTasks EnterPrivateHouse = new();
    public static readonly EnterFcHouseNamedTasks EnterFcHouseNamedTasks = new();

    /**
     * Enqueues the given action for later execution, this is helpful if the conditions
     * being checked within the action to determine if it should actually run or not
     * can be changed by actions that were run before it.
     */
    public static void EnqueueAction(BaseNamedTasks namedTasks, params object[] args)
    {
        AWC.TaskManager.Enqueue(() =>
            {
                namedTasks.Invoke(args);

                return true;
            }, $"ActionInstance: enqueued action {namedTasks.GetType().Name}");
    }
}
