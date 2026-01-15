using System;
using System.Numerics;
using AutoWeeklyCap.IPC;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace AutoWeeklyCap.Helpers;

public class RepairNPCHelper
{
    private static bool seenAddon = false;
    private static unsafe AtkUnitBase* addonRepair = null;
    private static unsafe AtkUnitBase* addonSelectYesno = null;
    private static unsafe AtkUnitBase* addonSelectIconString = null;

    public static bool Repair() => Repair(AutoWeeklyCap.Config.RepairPercentage);

    public static bool Repair(uint percent)
    {
        if (InventoryHelper.GetItemsNeedingRepairCount(percent) == 0)
            return false;

        if (!AutoWeeklyCap.PlayerState.IsLoaded || !Player.Available)
            return false;

        if (Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51])
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

            Chat.RunCommand("automove off");

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
                    var vendor = RepairVendorGameObject();
                    if (vendor == null)
                        return false;

                    if (GenericHelpers.TryGetAddonByName("SelectIconString", out addonSelectIconString) && GenericHelpers.IsAddonReady(addonSelectIconString))
                    {
                        AddonHelper.ClickSelectIconString(0);
                    }
                    else if (!GenericHelpers.TryGetAddonByName("Repair", out addonRepair) && !GenericHelpers.TryGetAddonByName("SelectYesno", out addonSelectYesno))
                    {
                        ObjectHelper.InteractWithObject(vendor);
                    }
                    else if (!seenAddon && (!GenericHelpers.TryGetAddonByName("SelectYesno", out addonSelectYesno) || !GenericHelpers.IsAddonReady(addonSelectYesno)))
                    {
                        AddonHelper.ClickRepair();
                    }
                    else if (GenericHelpers.TryGetAddonByName("SelectYesno", out addonSelectYesno) && GenericHelpers.IsAddonReady(addonSelectYesno))
                    {
                        AddonHelper.ClickSelectYesno();
                        seenAddon = true;
                    }
                    else if (seenAddon && (!GenericHelpers.TryGetAddonByName("SelectYesno", out addonSelectYesno) || !GenericHelpers.IsAddonReady(addonSelectYesno)))
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

    public static string RepairVendorAetheriteName => PlayerHelper.GetGrandCompany() switch
    {
        GrandCompany.Maelstrom => "The Aftcastle",
        GrandCompany.TwinAdder => "New Gridania",
        _ => "Steps of Nald",
    };

    private static IGameObject? RepairVendorGameObject()
    {
        try
        {
            var wantedDataId = RepairVendorDataId;
            IGameObject? closest = null;
            var closestDistance = float.MaxValue;

            foreach (var obj in Svc.Objects)
            {
                if (obj.ObjectKind is not (ObjectKind.EventNpc or ObjectKind.BattleNpc or ObjectKind.EventObj))
                    continue;

                if (obj.BaseId != wantedDataId)
                    continue;

                var d = Vector3.Distance(Player.Position, obj.Position);
                if (d < closestDistance)
                {
                    closest = obj;
                    closestDistance = d;
                }
            }

            if (closest != null)
                return closest;

            // Fallback: nearest object around the expected mender location.
            foreach (var obj in Svc.Objects)
            {
                if (obj == null)
                    continue;

                if (obj.ObjectKind is not (ObjectKind.EventNpc or ObjectKind.BattleNpc or ObjectKind.EventObj))
                    continue;

                var d = Vector3.Distance(RepairVendorLocation, obj.Position);
                if (d <= 6f)
                    return obj;
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return null;
    }

    private static unsafe void ResetRepairState()
    {
        seenAddon = false;
        addonRepair = null;
        addonSelectYesno = null;
        addonSelectIconString = null;
    }
}
