using System;
using AutoWeeklyCap.Helpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Runner.Actions;

public class SelfRepairAction : BaseAction
{
    protected override string Name => nameof(SelfRepairAction);
    protected override string[] AddonsToClose { get; } = ["SelectYesno", "SelectIconString", "Repair", "SelectString"];

    protected override bool Run()
    {
        var percent = AutoWeeklyCap.Config.RepairPercentage;
        if (!InventoryHelper.CanRepair(percent))
            return false;

        if (!PlayerHelper.CanSelfRepairWithCrafters)
        {
            AutoWeeklyCap.Log.Debug("switching to NPC repair, reason: player does not have all the required crafters leveled");
            return ActionInstance.NpcRepair.Invoke();
        }

        if (InventoryHelper.GetItemsNeedingRepairCount(percent) > InventoryHelper.GetDarkMatterCount())
        {
            AutoWeeklyCap.Log.Debug("switching to NPC repair, reason: too low quantity of dark matter");
            return ActionInstance.NpcRepair.Invoke();
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
