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

    // Safe-zone instances
    public static readonly SafezoneAction Safezone = new();

    public static readonly EnterGrandCompanyInnAction EnterGrandCompanyInn = new();
    public static readonly LeaveGrandCompanyInnAction LeaveGrandCompanyInn = new();
    public static readonly EnterApartmentAction EnterApartmentAction = new();
    public static readonly EnterPrivateHouseAction EnterPrivateHouse = new();
    public static readonly EnterFcHouseAction EnterFcHouseAction = new();
}
