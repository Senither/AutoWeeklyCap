using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Callback = ECommons.Automation.Callback;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace AutoWeeklyCap.Helpers;

public static unsafe class AddonHelper
{
    internal static bool IsTitleScreenReady()
    {
        try
        {
            return GenericHelpers.TryGetAddonByName<AtkUnitBase>("_TitleMenu", out var title)
                   && GenericHelpers.IsAddonReady(title)
                   && title->UldManager.NodeListCount > 3
                   && title->UldManager.NodeList[7]->IsVisible()
                   && title->UldManager.NodeList[3]->Color.A == 0xFF
                   && !GenericHelpers.TryGetAddonByName<AtkUnitBase>("TitleDCWorldMap", out _)
                   && !GenericHelpers.TryGetAddonByName<AtkUnitBase>("TitleConnect", out _);
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal static void FireCallBack(AtkUnitBase* addon, bool boolValue, params object[] args)
    {
        if (addon == null)
            return;

        try
        {
            Callback.Fire(addon, boolValue, args);
        }
        catch (Exception ex)
        {
            AWC.Log.Error($"{ex}");
        }
    }

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

    internal static bool ClickSelectString(int index)
    {
        if (!EzThrottler.Throttle(nameof(ClickSelectString), 500))
            return false;

        if (!TryGetReadyAddon("SelectString", out var addon))
            return false;

        var values = stackalloc AtkValue[2];
        values[0].Type = ValueType.Int;
        values[0].Int = index;
        values[1].Type = ValueType.Int;
        values[1].Int = 0;

        addon->FireCallback(2, values);
        return true;
    }

    internal static bool ClickSelectIconString(int index)
    {
        if (!EzThrottler.Throttle(nameof(ClickSelectIconString), 500))
            return false;

        if (!TryGetReadyAddon("SelectIconString", out var addon))
            return false;

        var values = stackalloc AtkValue[2];
        values[0].Type = ValueType.Int;
        values[0].Int = index;
        values[1].Type = ValueType.Int;
        values[1].Int = 0;

        addon->FireCallback(2, values);
        return true;
    }

    internal static bool ClickShopExchangeItem(int index, int quantity = 1)
    {
        if (!EzThrottler.Throttle(nameof(ClickShopExchangeItem), 500))
            return false;

        if (!TryGetReadyAddon("ShopExchangeCurrency", out var addon))
            return false;

        var values = stackalloc AtkValue[3];
        values[0].Type = ValueType.Int;
        values[0].Int = 0;
        values[1].Type = ValueType.Int;
        values[1].Int = index;
        values[2].Type = ValueType.Int;
        values[2].Int = quantity;

        addon->FireCallback(3, values);
        return true;
    }

    internal static bool ClickDialogueOk()
    {
        if (!EzThrottler.Throttle(nameof(ClickDialogueOk), 500))
            return false;

        if (!TryGetReadyAddon("Dialogue", out var addon))
            return false;

        var dialogue = new AddonMaster.Dialogue(addon);

        dialogue.Ok();

        return true;
    }

    internal static bool IsLobbyErrorVisible() => TryGetLobbyError(out _);

    internal static bool TryGetLobbyError(out AtkUnitBase* addon)
    {
        addon = null;

        if (TryGetReadyAddon("Dialogue", out var dialogue))
        {
            addon = dialogue;
            return true;
        }

        foreach (var name in new[] { "_TitleError", "TitleError", "TitleServerError", "TitleNetworkError" })
        {
            if (TryGetReadyAddon(name, out var errorAddon))
            {
                addon = errorAddon;
                return true;
            }
        }

        return false;
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
