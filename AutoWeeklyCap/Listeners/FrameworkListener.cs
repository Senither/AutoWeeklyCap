using System.Text.RegularExpressions;

using ECommons.UIHelpers.AddonMasterImplementations;

using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace AutoWeeklyCap.Listeners;

public static partial class FrameworkListener
{
    private static long _enforceUpdateStateAt = 0;

    public static void OnFrameworkUpdate(IFramework _)
    {
        AWC.Instance.DtrStatusBar.Draw();

        var unixNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (_enforceUpdateStateAt > unixNow) {
            return;
        }

        _enforceUpdateStateAt = unixNow + 500;

        AttemptErrorRecovery();
        UpdateRunnerLoop();
        DisableTitleScreenMovie();
    }

    private static void AttemptErrorRecovery()
    {
        if (!AWC.Config.AttemptRecoveryFromDisconnects || !ClientListener.IsRecoveringFromDisconnect) {
            return;
        }

        if (PlayerHelper.IsValid) {
            ClientListener.IsRecoveringFromDisconnect = false;

            if (!AWC.Runner.IsRunning()) {
                AWC.Runner.Start();
            }

            return;
        }

        if (AddonHelper.IsLobbyErrorVisible()) {
            AWC.Log.Debug($"Network: Lobby error detected (likely 2002), attempting to reconnect");
            ClientListener.EnqueueRestart();

            return;
        }

        if (AWC.TaskManager.IsBusy || LifestreamIPC.IsBusy()) {
            return;
        }

        if ((DateTime.UtcNow - ClientListener.LastRecoveryTimestamp).Seconds < 45) {
            return;
        }

        if (AddonHelper.IsTitleScreenReady()) {
            ClientListener.EnqueueRestart();
            return;
        }

        if (!EzThrottler.Throttle("RecoveryFromDisconnect.AddonAttempt", 250)) {
            return;
        }

        try {
            unsafe {
                if (AddonHelper.ClickSelectYesno()) {
                    AWC.Log.Debug("Network: Found Selectyesno addon, clicked yes");
                    return;
                }

                if (AddonHelper.TryGetReadyAddon("SelectOk", out var selectAddon)) {
                    var select = new AddonMaster.SelectOk(selectAddon);
                    if (!LoginQueueRegex().IsMatch(select.Text.Trim())) {
                        AWC.Log.Debug($"Network: Found SelectOk addon that is not queue, clicking OK button");
                        select.Ok();
                    }

                    AWC.Log.Debug($"Network: Found SelectOk addon is queue, doing nothing");
                    return;
                }

                if (AddonHelper.TryGetReadyAddon("_CharaSelectReturn", out var returnAddon)) {
                    AWC.Log.Debug($"Network: Found _CharaSelectReturn addon, returning to main title screen");

                    var returnToTitle = new AddonMaster.Dialogue(returnAddon);
                    returnToTitle.Ok();
                }
            }
        } catch (Exception) {
            // ignored
        }
    }

    private static void UpdateRunnerLoop()
    {
        CurrencyHelper.UpdateWeeklyAcquiredTomestonesForCurrentCharacter();
        PlayerHelper.UpdateJobLevelsForCurrentCharacter();

        AWC.Runner.Tick();
    }

    private static void DisableTitleScreenMovie()
    {
        if (!AWC.Config.DisableTitleScreenMovie) {
            return;
        }

        if (PlayerHelper.IsValid) {
            return;
        }

        if (!AddonHelper.IsTitleScreenReady()) {
            return;
        }

        try {
            unsafe {
                AgentLobby.Instance()->IdleTime = 0;
            }
        } catch (Exception) {
            // ignored
        }
    }

    [GeneratedRegex(@":\s*\d+\.$")]
    private static partial Regex LoginQueueRegex();
}
