using System.Numerics;
using AutoWeeklyCap.Helpers;
using AutoWeeklyCap.IPC;
using ECommons;
using ECommons.Automation.NeoTaskManager;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;

// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.Runner.Actions;

public class AutoSpendTomestoneAction : BaseAction
{
    // Path 7.4 - Zircon @ Solution Nine (Nexus Arcade)
    private static readonly Vector3 VendorPosition = new(-185.5f, 0.6600001f, -28.45f);
    private const uint VendorDataID = 1049079u;
    private const uint VendorTerritoryID = 1186u;
    private const int VendorSelectionIndex = 3;
    private const string VendorAetheriteName = "Nexus Arcade";

    // Pointers for game instances to open and interact with windows 
    private unsafe AtkUnitBase* AddonSelectIconString = null;
    private unsafe AtkUnitBase* AddonShopExchangeCurrency = null;

    protected override bool Run()
    {
        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled)
            return false;

        // TODO: Check for a valid config setup to buy tomestones with

        var longTask = new TaskManagerConfiguration(timeLimitMS: 120_000);

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (EzThrottler.Throttle("NavigatingToTomestoneTerritory", 500))
                return false;

            if (Player.Territory.RowId == VendorTerritoryID)
                return true;

            if (LifestreamIPC.IsBusy())
                return false;

            LifestreamIPC.ExecuteCommand(VendorAetheriteName);

            return true;
        }, "auto spend tomestone: start moving to territory");

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (EzThrottler.Throttle("NavigatingToTomestoneTerritory", 500))
                return false;

            return Player.Territory.RowId == VendorTerritoryID && PlayerHelper.IsReady && !LifestreamIPC.IsBusy();
        }, "auto spend tomestone: waiting for player to be in territory");

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (VNavMeshIPC.IsRunning() || !VNavMeshIPC.IsReady())
                return false;

            ChatHelper.RunCommand("automove off");

            VNavMeshIPC.SetTolerance(.25f);
            VNavMeshIPC.SetAlignCamera(true);
            VNavMeshIPC.PathfindAndMoveTo(VendorPosition, false);

            return true;
        }, "auto spend tomestone: start moving to npc location", longTask);

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            var distance = Vector3.Distance(VendorPosition, Player.Position);
            if (distance >= .50)
                return false;

            VNavMeshIPC.Stop();

            return true;
        }, "auto spend tomestone: waiting for player movement to npc", longTask);

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (EzThrottler.Throttle("OpeningTomestoneVendorWindow", 250))
                return false;

            var vendor = ObjectHelper.FindGameObject(VendorDataID, VendorPosition);
            if (vendor == null)
                return false;

            unsafe
            {
                if (GenericHelpers.TryGetAddonByName("SelectIconString", out AddonSelectIconString) && GenericHelpers.IsAddonReady(AddonSelectIconString))
                    AddonHelper.ClickSelectIconString(VendorSelectionIndex);
                else if (GenericHelpers.TryGetAddonByName("ShopExchangeCurrency", out AddonShopExchangeCurrency) && GenericHelpers.IsAddonReady(AddonShopExchangeCurrency))
                    return true;
                else if (!GenericHelpers.TryGetAddonByName("SelectIconString", out AddonSelectIconString))
                    ObjectHelper.InteractWithObject(vendor);
            }

            return false;
        }, "auto spend tomestone: open window");

        return true;
    }
}
