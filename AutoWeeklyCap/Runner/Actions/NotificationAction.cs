namespace AutoWeeklyCap.Runner.Actions;

public class NotificationAction : BaseAction
{
    protected override string Name => nameof(NotificationAction);

    protected override bool Run(params object[] args)
    {
        if (!NotificationMasterIPC.IsEnabled) {
            return false;
        }

        if (args.Length == 0 || args[0] is not StopNotificationType) {
            return false;
        }

        var type = (StopNotificationType)args[0];
        LogDebug($"Called for type: {type}");

        EnqueueDelay(500);

        if (AWC.Config.NotificationMasterUsingFlashTaskbarIcon) {
            Enqueue(NotificationMasterIPC.SendFlashTaskbarIcon, "Flash taskbar icon");
        }

        if (AWC.Config.NotificationMasterUsingToastNotification) {
            Enqueue(
                () => NotificationMasterIPC.SendDisplayToastNotification(
                    Constants.Name, type.GetMessage()
                ), "Toast notification"
            );
        }

        if (AWC.Config.NotificationMasterUsingPlaySound && AWC.Config.NotificationMasterUsingPlaySoundOptionFilePath.Length > 0) {
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
