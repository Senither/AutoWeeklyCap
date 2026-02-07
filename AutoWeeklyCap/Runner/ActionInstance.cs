using AutoWeeklyCap.Runner.Actions;

namespace AutoWeeklyCap.Runner;

public static class ActionInstance
{
    public static readonly ExtractAction Extract = new();
    public static readonly AutoSpendTomestoneAction SpendTomestone = new();
    public static readonly SelfRepairAction SelfRepair = new();
    public static readonly NpcRepairAction NpcRepair = new();
    public static readonly DeliverooAction Deliveroo = new();
    public static readonly NotificationAction Notification = new();
}
