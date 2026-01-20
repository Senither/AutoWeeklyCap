using System;
using System.Numerics;
using AutoWeeklyCap.Helpers;
using AutoWeeklyCap.IPC;
using Dalamud.Game.ClientState.Conditions;
using ECommons;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoWeeklyCap.Runner.Actions;

public class NpcRepairAction : BaseAction
{
    protected override string Name => nameof(NpcRepairAction);
    protected override string[] AddonsToClose { get; } = ["SelectYesno", "SelectIconString", "Repair", "SelectString"];

    private static bool SeenAddon = false;
    private static unsafe AtkUnitBase* AddonRepair = null;
    private static unsafe AtkUnitBase* AddonSelectYesno = null;
    private static unsafe AtkUnitBase* AddonSelectIconString = null;

    protected override bool Run()
    {
        var percent = AutoWeeklyCap.Config.RepairPercentage;
        if (InventoryHelper.GetItemsNeedingRepairCount(percent) == 0)
            return false;

        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled)
            return false;

        var longTask = new TaskManagerConfiguration(timeLimitMS: 120_000);
        ResetRepairState();

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (EzThrottler.Throttle("NavigatingToGcTerritory", 500))
                return false;

            if (Player.Territory.RowId == RepairVendorTerritoryType())
                return true;

            if (LifestreamIPC.IsBusy())
                return false;

            LifestreamIPC.ExecuteCommand(RepairVendorAetheriteName);

            return true;
        }, "npc repair: start moving to gc territory");

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (EzThrottler.Throttle("NavigatingToGcTerritory", 500))
                return false;

            return Player.Territory.RowId == RepairVendorTerritoryType() && PlayerHelper.IsReady;
        }, "npc repair: waiting for player to be in gc territory");

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (VNavMeshIPC.IsRunning() || !VNavMeshIPC.IsReady())
                return false;

            ChatHelper.RunCommand("automove off");

            VNavMeshIPC.SetTolerance(.25f);
            VNavMeshIPC.SetAlignCamera(true);
            VNavMeshIPC.PathfindAndMoveTo(RepairVendorLocation, false);

            return true;
        }, "npc repair: start moving to npc location", longTask);

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            var distance = Vector3.Distance(RepairVendorLocation, Player.Position);
            if (distance > 1.25)
                return false;

            VNavMeshIPC.Stop();

            return true;
        }, "npc repair: waiting for player movement to npc", longTask);

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (EzThrottler.Throttle("RepairingGearViaNPC", 250))
                return false;

            try
            {
                unsafe
                {
                    var vendor = ObjectHelper.FindGameObject(RepairVendorDataId, RepairVendorLocation);
                    if (vendor == null)
                        return false;

                    if (GenericHelpers.TryGetAddonByName("SelectIconString", out AddonSelectIconString) && GenericHelpers.IsAddonReady(AddonSelectIconString))
                    {
                        AddonHelper.ClickSelectIconString(0);
                    }
                    else if (!GenericHelpers.TryGetAddonByName("Repair", out AddonRepair) && !GenericHelpers.TryGetAddonByName("SelectYesno", out AddonSelectYesno))
                    {
                        ObjectHelper.InteractWithObject(vendor);
                    }
                    else if (!SeenAddon && (!GenericHelpers.TryGetAddonByName("SelectYesno", out AddonSelectYesno) || !GenericHelpers.IsAddonReady(AddonSelectYesno)))
                    {
                        AddonHelper.ClickRepair();
                    }
                    else if (GenericHelpers.TryGetAddonByName("SelectYesno", out AddonSelectYesno) && GenericHelpers.IsAddonReady(AddonSelectYesno))
                    {
                        AddonHelper.ClickSelectYesno();
                        SeenAddon = true;
                    }
                    else if (SeenAddon && (!GenericHelpers.TryGetAddonByName("SelectYesno", out AddonSelectYesno) || !GenericHelpers.IsAddonReady(AddonSelectYesno)))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }, "npc repair: repair gear");

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
                    {
                        ResetRepairState();
                        return true;
                    }

                    repairAddon->Close(true);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }, "npc repair: close window");

        return true;
    }

    private static uint RepairVendorTerritoryType()
    {
        return PlayerHelper.GetGrandCompanyTerritoryType(PlayerHelper.GetGrandCompany());
    }

    private static Vector3 RepairVendorLocation => PlayerHelper.GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => new Vector3(17.715698f, 40.200005f, 3.9520264f),
        GrandCompany.TwinAdder => new Vector3(24.826416f, -8f, 93.18677f),
        _ => new Vector3(32.85266f, 6.999999f, -81.31531f),
    };

    private static uint RepairVendorDataId => PlayerHelper.GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => 1003251u,
        GrandCompany.TwinAdder => 1000394u,
        _ => 1004416u,
    };

    private static string RepairVendorAetheriteName => PlayerHelper.GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => "The Aftcastle",
        GrandCompany.TwinAdder => "New Gridania",
        _ => "Steps of Nald",
    };

    private static unsafe void ResetRepairState()
    {
        SeenAddon = false;
        AddonRepair = null;
        AddonSelectYesno = null;
        AddonSelectIconString = null;
    }
}
