namespace AutoWeeklyCap.Runner.Actions;

public class NotificationAction : BaseAction
{
    protected override string Name => nameof(NotificationAction);

    protected override bool Run(params object[] args)
    {
        if (!AWC.Config.NotificationMasterEnabled || !NotificationMasterIPC.IsEnabled)
            return false;

        EnqueueDelay(500);

        if (AWC.Config.NotificationMasterUsingFlashTaskbarIcon)
        {
            Enqueue(NotificationMasterIPC.SendFlashTaskbarIcon, "Flash taskbar icon");
        }

        if (AWC.Config.NotificationMasterUsingToastNotification)
        {
            Enqueue(() => NotificationMasterIPC.SendDisplayToastNotification(
                        AWC.Name,
                        "The runner has finished capping tomes on all your characters."
                    ), "Toast notification");
        }

        if (AWC.Config.NotificationMasterUsingPlaySound && AWC.Config.NotificationMasterUsingPlaySoundOptionFilePath.Length > 0)
        {
            Enqueue(() => NotificationMasterIPC.SendPlaySound(
                        AWC.Config.NotificationMasterUsingPlaySoundOptionFilePath,
                        AWC.Config.NotificationMasterUsingPlaySoundOptionVolume / 100f,
                        AWC.Config.NotificationMasterUsingPlaySoundOptionRepeat,
                        AWC.Config.NotificationMasterUsingPlaySoundOptionStopOnFocus
                    ), "Play sound");
        }

        return true;
    }
}
