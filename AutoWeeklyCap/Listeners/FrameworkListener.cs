using System;
using System.Text.RegularExpressions;
using AutoWeeklyCap.Helpers;
using AutoWeeklyCap.IPC;
using Dalamud.Plugin.Services;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;

namespace AutoWeeklyCap.Listeners;

public partial class FrameworkListener
{
    protected long EnforceUpdateStateAt = 0;

    public void OnFrameworkUpdate(IFramework _)
    {
        AutoWeeklyCap.Instance.DtrStatusBar.Draw();

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

        if (!EzThrottler.Throttle("RecoveryFromDisconnect.AddonAttempt", 250))
            return;

        try
        {
            unsafe
            {
                if (AddonHelper.ClickSelectYesno())
                {
                    AutoWeeklyCap.Log.Debug("Found Selectyesno addon, clicked yes");
                    return;
                }

                if (AddonHelper.TryGetReadyAddon("SelectOk", out var selectAddon))
                {
                    var select = new AddonMaster.SelectOk(selectAddon);
                    if (!LoginQueueRegex().IsMatch(select.Text.Trim()))
                    {
                        AutoWeeklyCap.Log.Debug($"Found SelectOk addon that is not queue, clicking OK button");
                        select.Ok();
                    }

                    AutoWeeklyCap.Log.Debug($"Found SelectOk addon is queue, doing nothing");
                    return;
                }

                if (AddonHelper.TryGetReadyAddon("_CharaSelectReturn", out var returnAddon))
                {
                    AutoWeeklyCap.Log.Debug($"Found _CharaSelectReturn addon, returning to main title screen");

                    var returnToTitle = new AddonMaster.Dialogue(returnAddon);
                    returnToTitle.Ok();
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

    [GeneratedRegex(@":\s*\d+\.$")]
    private static partial Regex LoginQueueRegex();
}
