using AutoWeeklyCap.Contracts.Runner;

using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoWeeklyCap.Runner.Actions;

public class NpcRepairAction : BaseAction
{
    protected override string Name => nameof(NpcRepairAction);
    protected override string[] AddonsToClose { get; } = ["SelectYesno", "SelectIconString", "Repair", "SelectString"];

    private const int LongTaskTimeout = 120_000;

    private static bool _seenAddon = false;
    private static unsafe AtkUnitBase* _addonSelectYesno = null;
    private static unsafe AtkUnitBase* _addonSelectIconString = null;

    // ReSharper disable once NotAccessedField.Local
    private static unsafe AtkUnitBase* _addonRepair = null;

    protected override bool Run(params object[] args)
    {
        if (InventoryHelper.GetItemsNeedingRepairCount(99) == 0) {
            return false;
        }

        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled) {
            return false;
        }

        LocationManager.Reset();

        ResetRepairState();

        ActionInstance.LeaveGrandCompanyInn.Invoke();

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.Blacksmith, "Repairing gear");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("NavigatingToGcTerritory", 500)) {
                return false;
            }

            if (Player.Territory.RowId == GrandCompanyHelper.TerritoryId) {
                return true;
            }

            if (LifestreamIPC.IsBusy()) {
                return false;
            }

            LifestreamIPC.ExecuteCommand(GrandCompanyHelper.AetheriteName);

            return true;
        }, "start moving to gc territory");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("NavigatingToGcTerritory", 500)) {
                return false;
            }

            return Player.Territory.RowId == GrandCompanyHelper.TerritoryId && PlayerHelper.IsReady;
        }, "waiting for player to be in gc territory");

        Enqueue(
            () => MovementHelper.MoveTo(GrandCompanyHelper.RepairVendorLocation),
            "start moving to npc location",
            LongTaskTimeout
        );

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("RepairingGearViaNPC", 250)) {
                return false;
            }

            try {
                unsafe {
                    var vendor = ObjectHelper.FindGameObject(GrandCompanyHelper.RepairVendorId, GrandCompanyHelper.RepairVendorLocation);
                    if (vendor == null) {
                        return false;
                    }

                    if (GenericHelpers.TryGetAddonByName("SelectIconString", out _addonSelectIconString) && GenericHelpers.IsAddonReady(_addonSelectIconString)) {
                        AddonHelper.ClickSelectIconString(0);
                    } else if (!GenericHelpers.TryGetAddonByName("Repair", out _addonRepair) && !GenericHelpers.TryGetAddonByName("SelectYesno", out _addonSelectYesno)) {
                        ObjectHelper.InteractWithObject(vendor);
                    } else if (!_seenAddon && (!GenericHelpers.TryGetAddonByName("SelectYesno", out _addonSelectYesno) || !GenericHelpers.IsAddonReady(_addonSelectYesno))) {
                        AddonHelper.ClickRepair();
                    } else if (GenericHelpers.TryGetAddonByName("SelectYesno", out _addonSelectYesno) && GenericHelpers.IsAddonReady(_addonSelectYesno)) {
                        AddonHelper.ClickSelectYesno();
                        _seenAddon = true;
                    } else if (_seenAddon && (!GenericHelpers.TryGetAddonByName("SelectYesno", out _addonSelectYesno) || !GenericHelpers.IsAddonReady(_addonSelectYesno))) {
                        return true;
                    }
                }
            } catch (Exception) {
                // ignored
            }

            return false;
        }, "repair gear");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("RepairClose", 250)) {
                return false;
            }

            try {
                unsafe {
                    if (AddonHelper.TryGetReadyAddon("SelectYesno", out _)) {
                        return false;
                    }

                    if (!AddonHelper.TryGetReadyAddon("Repair", out var repairAddon)) {
                        ResetRepairState();
                        return true;
                    }

                    repairAddon->Close(true);
                }
            } catch (Exception) {
                // ignored
            }

            return false;
        }, "close window");

        return true;
    }

    private static unsafe void ResetRepairState()
    {
        _seenAddon = false;
        _addonRepair = null;
        _addonSelectYesno = null;
        _addonSelectIconString = null;
    }
}
