using AutoWeeklyCap.Contracts.Runner;
using AutoWeeklyCap.Runner.Actions;
using AutoWeeklyCap.Runner.Actions.Safezones;

namespace AutoWeeklyCap.Runner;

public static class ActionInstance
{
    public static readonly ExtractAction Extract = new();
    public static readonly AutoSpendTomestoneAction SpendTomestone = new();
    public static readonly SelfRepairAction SelfRepair = new();
    public static readonly NpcRepairAction NpcRepair = new();
    public static readonly ReturnToHomeworldAction Homeworld = new();
    public static readonly DeliverooAction Deliveroo = new();
    public static readonly NotificationAction Notification = new();
    public static readonly EquipGearUpgradeAction EquipGearUpgrade = new();
    public static readonly BuyLevelingUpgradeAction BuyLevelingUpgrade = new();
    public static readonly MoveInventoryItemsToSaddlebagAction MoveInventoryItemsToSaddlebag = new();
    public static readonly UseFoodAction UseFood = new();
    public static readonly BuyFoodAction BuyFood = new();
    public static readonly LearnTripleTriadCardAction LearnTripleTriadCard = new();
    public static readonly SellTripleTriadCardAction SellTripleTriadCardAction = new();

    // Safe-zone instances
    public static readonly SafezoneAction Safezone = new();

    public static readonly EnterGrandCompanyInnAction EnterGrandCompanyInn = new();
    public static readonly LeaveGrandCompanyInnAction LeaveGrandCompanyInn = new();
    public static readonly EnterApartmentAction EnterApartment = new();
    public static readonly EnterPrivateHouseAction EnterPrivateHouse = new();
    public static readonly EnterFcHouseAction EnterFcHouse = new();

    /**
     * Enqueues the given action for later execution, this is helpful if the conditions
     * being checked within the action to determine if it should actually run or not
     * can be changed by actions that were run before it.
     */
    public static void EnqueueAction(BaseAction action, params object[] args)
    {
        AWC.TaskManager.Enqueue(() =>
            {
                action.Invoke(args);

                return true;
            }, $"ActionInstance: enqueued action {action.GetType().Name}");
    }
}
