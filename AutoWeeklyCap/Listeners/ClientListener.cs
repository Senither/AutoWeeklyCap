namespace AutoWeeklyCap.Listeners;

public class ClientListener
{
    public static bool IsRecoveringFromDisconnect = false;
    public static bool IsRestarting = false;
    public static DateTime LastRecoveryTimestamp = DateTime.MinValue;

    public void OnLogout(int type, int code)
    {
        if (!AWC.Config.AttemptRecoveryFromDisconnects)
            return;

        if (!IsDisconnectErrorCode(code))
            return;

        AWC.Log.Debug($"Disconnection detected, runner status: {(AWC.Runner.IsRunning() ? "active" : "idle")}");
        if (!AWC.Runner.IsRunning())
            return;

        EnqueueRestart();
    }

    public static void EnqueueRestart()
    {
        if (IsRestarting && (DateTime.UtcNow - LastRecoveryTimestamp).Seconds < 10)
            return;

        IsRecoveringFromDisconnect = true;
        IsRestarting = true;
        LastRecoveryTimestamp = DateTime.UtcNow;

        AWC.Log.Debug($"Queueing up restart tasks to recover from disconnect");

        AWC.Runner.Abort();
        AWC.TaskManager.Enqueue(() =>
        {
            try
            {
                unsafe
                {
                    if (!EzThrottler.Throttle("AttemptDisconnectedRecovery", 250))
                        return false;

                    if (AddonHelper.TryGetLobbyError(out var errorAddon) && errorAddon->IsVisible)
                    {
                        var dialogueStatus = AddonHelper.ClickDialogueOk();

                        AWC.Log.Debug($"Found lobby error [Addon: {errorAddon->GetType()}, Click: {dialogueStatus}]");
                        return false;
                    }

                    return !AddonHelper.IsLobbyErrorVisible() && AddonHelper.IsTitleScreenReady();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }, "close disconnect dialogs");

        AWC.TaskManager.Enqueue(() =>
        {
            if (!AddonHelper.IsTitleScreenReady())
                return false;

            AWC.Runner.Start();
            IsRestarting = false;

            return true;
        }, "restarting runner");
    }

    private static bool IsDisconnectErrorCode(int code)
    {
        return code is 90001 or 90002 or 90006 or 90007;
    }
}
