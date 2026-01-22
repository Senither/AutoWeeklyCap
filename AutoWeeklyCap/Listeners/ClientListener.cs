using System;
using AutoWeeklyCap.Helpers;
using ECommons.Throttlers;

namespace AutoWeeklyCap.Listeners;

public class ClientListener
{
    public void OnLogout(int type, int code)
    {
        if (!AutoWeeklyCap.Config.AttemptRecoveryFromDisconnects)
            return;

        if (!IsDisconnectErrorCode(code))
            return;

        AutoWeeklyCap.Log.Debug($"Disconnection detected, runner status: {(AutoWeeklyCap.Runner.IsRunning() ? "active" : "idle")}");
        if (!AutoWeeklyCap.Runner.IsRunning())
            return;

        AutoWeeklyCap.Runner.Abort();

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            try
            {
                unsafe
                {
                    if (!EzThrottler.Throttle("AttemptDisconnectedRecovery", 250))
                        return false;

                    if (AddonHelper.TryGetLobbyError(out var errorAddon))
                    {
                        AutoWeeklyCap.Log.Debug($"Found lobby error [Addon: {errorAddon->GetType()}, Click: {AddonHelper.ClickDialogueOk()}]");
                        return false;
                    }

                    var status = !AddonHelper.IsLobbyErrorVisible() && AddonHelper.IsTitleScreenReady();

                    return status;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }, "close disconnect dialogs");

        AutoWeeklyCap.TaskManager.Enqueue(() =>
        {
            if (!AddonHelper.IsTitleScreenReady())
                return false;

            AutoWeeklyCap.Runner.Start();

            return true;
        }, "restarting runner");
    }

    private bool IsDisconnectErrorCode(int code)
    {
        return code is 90001 or 90002 or 90006 or 90007;
    }
}
