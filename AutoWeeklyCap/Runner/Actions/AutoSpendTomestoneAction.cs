using System;
using System.Numerics;
using AutoWeeklyCap.Helpers;
using AutoWeeklyCap.IPC;
using Dalamud.Game.ClientState.Conditions;
using ECommons;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
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
        var itemToBuy = TomestoneItemHelper.GetTomestoneItemFromName(AutoWeeklyCap.Config.SpendUncappedTomestoneItemName);
        if (itemToBuy == null)
            return false;

        var quantity = itemToBuy.CalculateQuantityForGivenTomestones(CurrencyHelper.GetUncappedAcquiredTomestoneCount());
        if (quantity == 0)
            return false;

        if (!AutoWeeklyCap.PlayerState.IsLoaded || !Player.Available)
            return false;

        if (Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51])
            return false;

        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled)
            return false;

        unsafe
        {
            if (InventoryManager.Instance()->GetEmptySlotsInBag() < 1)
                return false;
        }

        var longTask = new TaskManagerConfiguration(timeLimitMS: 120_000);

        // Reset state before starting
        unsafe
        {
            AddonSelectIconString = null;
            AddonShopExchangeCurrency = null;
        }

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

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (EzThrottler.Throttle("BuyingTomestoneItem", 500))
                return false;

            unsafe
            {
                if (GenericHelpers.TryGetAddonByName("SelectYesno", out AddonSelectIconString) && GenericHelpers.IsAddonReady(AddonSelectIconString))
                {
                    AddonHelper.ClickSelectYesno();
                    return true;
                }

                if (GenericHelpers.TryGetAddonByName("ShopExchangeCurrency", out AddonShopExchangeCurrency) && GenericHelpers.IsAddonReady(AddonShopExchangeCurrency))
                    AddonHelper.ClickShopExchangeItem(itemToBuy.Index, quantity);
            }

            return false;
        }, "auto spend tomestone: buy tomestone item");

        AutoWeeklyCap.TaskManager.EnqueueDelay(500);
        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (EzThrottler.Throttle("BuyingTomestoneItemClose", 500))
                return false;

            try
            {
                unsafe
                {
                    if (AddonHelper.TryGetReadyAddon("SelectYesno", out _))
                        return false;

                    if (!AddonHelper.TryGetReadyAddon("ShopExchangeCurrency", out var addonShopExchangeCurrency))
                        return true;

                    addonShopExchangeCurrency->Close(true);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }, "auto spend tomestone: close window");

        return true;
    }
}
