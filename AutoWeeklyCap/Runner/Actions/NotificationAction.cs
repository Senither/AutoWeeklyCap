namespace AutoWeeklyCap.Runner.Actions;

public enum NotificationType
{
    RunnerStopped = 0,
    CharacterCapped = 1
}

public static class NotificationTypeExtensions
{
    public static string GetMessage(this NotificationType notificationType)
    {
        return notificationType switch
        {
            NotificationType.CharacterCapped => "The runner has finished capping tomes on all your characters.",
            NotificationType.RunnerStopped => "The runner has finished a duty tomestone run and has stopped.",
            _ => throw new ArgumentOutOfRangeException(nameof(notificationType), notificationType, null)
        };
    }
}

public class NotificationAction : BaseAction
{
    protected override string Name => nameof(NotificationAction);

    protected override bool Run(params object[] args)
    {
        if (!NotificationMasterIPC.IsEnabled)
            return false;

        if (args.Length == 0 || args[0] is not NotificationType)
            return false;

        var type = (NotificationType)args[0];
        LogDebug($"Called for type: {type}");

        EnqueueDelay(500);

        if (AWC.Config.NotificationMasterUsingFlashTaskbarIcon)
        {
            Enqueue(NotificationMasterIPC.SendFlashTaskbarIcon, "Flash taskbar icon");
        }

        if (AWC.Config.NotificationMasterUsingToastNotification)
        {
            Enqueue(() => NotificationMasterIPC.SendDisplayToastNotification(
                        AWC.Name, type.GetMessage()), "Toast notification"
            );
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
