using System;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Helpers;

public static class RepairHelper
{
    public static bool Repair() => Repair(AutoWeeklyCap.Config.RepairPercentage);

    public static bool Repair(uint percent)
    {
        if (!InventoryHelper.CanRepair(percent))
            return false;

        if (InventoryHelper.GetItemsNeedingRepairCount(percent) > InventoryHelper.GetDarkMatterCount())
        {
            AutoWeeklyCap.Log.Debug($"Repair attempt reverting to NPC due to lack of dark matter");
            return RepairNPCHelper.Repair(percent);
        }

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (!EzThrottler.Throttle("RepairOpen", 250))
                return false;
            
            try
            {
                unsafe
                {
                    if (AddonHelper.TryGetReadyAddon("Repair", out _))
                        return true;

                    ActionManager.Instance()->UseAction(ActionType.GeneralAction, 6);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }, "self repair: open window");

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            try
            {
                unsafe
                {
                    if (!AddonHelper.TryGetReadyAddon("Repair", out _))
                        return false;

                    if (AddonHelper.TryGetReadyAddon("SelectYesno", out _))
                    {
                        AddonHelper.ClickSelectYesno();
                        return true;
                    }

                    if (!InventoryHelper.CanRepair(percent))
                        return true;

                    if (EzThrottler.Throttle("RepairAll", 1000))
                        AddonHelper.ClickRepair();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }, "self repair: repair all + confirm");

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (!EzThrottler.Throttle("RepairClose", 250))
                return false;

            try
            {
                unsafe
                {
                    if (AddonHelper.TryGetReadyAddon("SelectYesno", out _))
                        return false;

                    if (!AddonHelper.TryGetReadyAddon("Repair", out var repairAddon))
                        return true;

                    repairAddon->Close(true);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }, "self repair: close window");

        return true;
    }
}
