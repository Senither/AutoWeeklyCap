using System;
using AutoWeeklyCap.Helpers;
using AutoWeeklyCap.IPC;
using Dalamud.Plugin.Services;
using ECommons.UIHelpers.AddonMasterImplementations;

namespace AutoWeeklyCap.Listeners;

public class FrameworkListener
{
    protected long EnforceUpdateStateAt = 0;

    public void OnFrameworkUpdate(IFramework _)
    {
        var unixNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (EnforceUpdateStateAt > unixNow)
            return;

        EnforceUpdateStateAt = unixNow + 500;

        AttemptErrorRecovery();
        UpdateRunnerLoop();
    }

    private static void AttemptErrorRecovery()
    {
        if (!AutoWeeklyCap.Config.AttemptRecoveryFromDisconnects || !ClientListener.IsRecoveringFromDisconnect)
            return;

        if (PlayerHelper.IsValid)
        {
            ClientListener.IsRecoveringFromDisconnect = false;

            if (!AutoWeeklyCap.Runner.IsRunning())
                AutoWeeklyCap.Runner.Start();

            return;
        }

        if (AddonHelper.IsLobbyErrorVisible())
        {
            AutoWeeklyCap.Log.Debug($"Lobby error detected (likely 2002), attempting to reconnect");
            ClientListener.EnqueueRestart();

            return;
        }

        if (AutoWeeklyCap.TaskManager.IsBusy || LifestreamIPC.IsBusy())
            return;

        if ((DateTime.UtcNow - ClientListener.LastRecoveryTimestamp).Seconds < 45)
            return;

        if (AddonHelper.IsTitleScreenReady())
        {
            ClientListener.EnqueueRestart();
            return;
        }

        try
        {
            unsafe
            {
                if (AddonHelper.ClickSelectYesno())
                    return;

                if (AddonHelper.TryGetReadyAddon("SelectOk", out var addon))
                {
                    var select = new AddonMaster.SelectOk(addon);
                    if (select.Text.Contains("Players in queue:"))
                        select.Ok();
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private static void UpdateRunnerLoop()
    {
        CurrencyHelper.UpdateWeeklyAcquiredTomestonesForCurrentCharacter();
        AutoWeeklyCap.Runner.Tick();
    }
}
