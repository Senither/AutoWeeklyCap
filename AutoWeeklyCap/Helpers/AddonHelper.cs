using ECommons;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoWeeklyCap.Helpers;

public static unsafe class AddonHelper
{
    internal static bool ClickSelectYesno(bool yes = true)
    {
        if (!EzThrottler.Throttle(nameof(ClickSelectYesno), 500))
            return false;

        if (!TryGetReadyAddon("SelectYesno", out var addon))
            return false;

        var selectYesno = new AddonMaster.SelectYesno(addon);
        
        if (yes)
            selectYesno.Yes();
        else
            selectYesno.No();

        return true;
    }

    internal static bool ClickRepair()
    {
        if (!TryGetReadyAddon("Repair", out var addon))
            return false;

        new AddonMaster.Repair(addon).RepairAll();
        
        return true;
    }

    internal static bool TryGetReadyAddon(string addonName, out AtkUnitBase* addon)
    {
        if (!GenericHelpers.TryGetAddonByName(addonName, out addon))
            return false;

        if (!GenericHelpers.IsAddonReady(addon))
            return false;

        if (Player.Character != null && Player.Character->IsCasting)
            return false;

        return true;
    }
}
