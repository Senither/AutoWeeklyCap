using System.Text.RegularExpressions;
using ECommons.UIHelpers.AddonMasterImplementations;

namespace AutoWeeklyCap.Listeners;

public partial class FrameworkListener
{
    protected long EnforceUpdateStateAt = 0;

    public void OnFrameworkUpdate(IFramework _)
    {
        AWC.Instance.DtrStatusBar.Draw();

        var unixNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (EnforceUpdateStateAt > unixNow)
            return;

        EnforceUpdateStateAt = unixNow + 500;

        AttemptErrorRecovery();
        UpdateRunnerLoop();
    }

    private static void AttemptErrorRecovery()
    {
        if (!AWC.Config.AttemptRecoveryFromDisconnects || !ClientListener.IsRecoveringFromDisconnect)
            return;

        if (PlayerHelper.IsValid)
        {
            ClientListener.IsRecoveringFromDisconnect = false;

            if (!AWC.Runner.IsRunning())
                AWC.Runner.Start();

            return;
        }

        if (AddonHelper.IsLobbyErrorVisible())
        {
            AWC.Log.Debug($"Lobby error detected (likely 2002), attempting to reconnect");
            ClientListener.EnqueueRestart();

            return;
        }

        if (AWC.TaskManager.IsBusy || LifestreamIPC.IsBusy())
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
                    AWC.Log.Debug("Found Selectyesno addon, clicked yes");
                    return;
                }

                if (AddonHelper.TryGetReadyAddon("SelectOk", out var selectAddon))
                {
                    var select = new AddonMaster.SelectOk(selectAddon);
                    if (!LoginQueueRegex().IsMatch(select.Text.Trim()))
                    {
                        AWC.Log.Debug($"Found SelectOk addon that is not queue, clicking OK button");
                        select.Ok();
                    }

                    AWC.Log.Debug($"Found SelectOk addon is queue, doing nothing");
                    return;
                }

                if (AddonHelper.TryGetReadyAddon("_CharaSelectReturn", out var returnAddon))
                {
                    AWC.Log.Debug($"Found _CharaSelectReturn addon, returning to main title screen");

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
        AWC.Runner.Tick();
    }

    [GeneratedRegex(@":\s*\d+\.$")]
    private static partial Regex LoginQueueRegex();
}
