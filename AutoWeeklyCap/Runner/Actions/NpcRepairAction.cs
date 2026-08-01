using AutoWeeklyCap.Contracts.Runner;

using ECommons.Configuration;

namespace AutoWeeklyCap.Runner.Actions;

public class NpcRepairAction : BaseAction
{
    protected override string Name => nameof(NpcRepairAction);
    protected override string[] AddonsToClose { get; } = ["SelectYesno", "SelectIconString", "Repair", "SelectString"];

    private const int LongTaskTimeout = 120_000;
    private const string MetricsKey = "RepairGil";

    protected override bool Run(params object[] args)
    {
        if (InventoryHelper.GetItemsNeedingRepairCount(99) == 0) {
            return false;
        }

        if (!VNavMeshIPC.IsEnabled || !LifestreamIPC.IsEnabled) {
            return false;
        }

        LocationManager.Reset();

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
            if (!EzThrottler.Throttle("OpeningRepairWindow", 250)) {
                return false;
            }

            try {
                unsafe {
                    AWC.Runner.State.SetMetric(MetricsKey, (uint)CurrencyHelper.GetGil());

                    var vendor = ObjectHelper.FindGameObject(GrandCompanyHelper.RepairVendorId, GrandCompanyHelper.RepairVendorLocation);
                    if (vendor == null) {
                        return false;
                    }

                    if (AddonHelper.TryGetReadyAddon("Repair", out _)) {
                        return true;
                    }

                    if (AddonHelper.TryGetReadyAddon("SelectIconString", out _)) {
                        AddonHelper.ClickSelectIconString(0);
                    } else {
                        ObjectHelper.InteractWithObject(vendor);
                    }
                }
            } catch (Exception) {
                // ignored
            }

            return false;
        }, "opening repair window");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("RepairingGearViaNPC", 250)) {
                return false;
            }

            try {
                unsafe {
                    if (!AddonHelper.TryGetReadyAddon("Repair", out _)) {
                        return true;
                    }

                    if (AddonHelper.TryGetReadyAddon("SelectYesno", out _)) {
                        AddonHelper.ClickSelectYesno();
                        return true;
                    }

                    AddonHelper.ClickRepair();
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
                        return true;
                    }

                    repairAddon->Close(true);
                }
            } catch (Exception) {
                // ignored
            }

            return false;
        }, "close window");

        Enqueue(() =>
        {
            var character = PlayerHelper.GetFullCharacterName();
            if (character == null) {
                return true;
            }

            uint gilSpent = 0;

            if (AWC.Runner.State.HasMetric(MetricsKey)) {
                uint before = AWC.Runner.State.PullMetric(MetricsKey);

                gilSpent = (uint)(before - CurrencyHelper.GetGil());
            }

            AWC.Config.GetOrRegisterCharacterOptions(character)
                ?.Metrics
                .IncrementRepairsCounter(gilSpent: gilSpent);

            EzConfig.Save();
            return true;
        }, "update metrics");

        return true;
    }
}
