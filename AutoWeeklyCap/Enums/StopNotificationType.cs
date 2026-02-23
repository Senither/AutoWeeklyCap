namespace AutoWeeklyCap.Enums;

public enum StopNotificationType
{
    RunnerStopped = 0,
    CharacterCapped = 1
}

public static class NotificationTypeExtensions
{
    public static string GetMessage(this StopNotificationType stopNotificationType)
    {
        return stopNotificationType switch
        {
            StopNotificationType.CharacterCapped => "The runner has finished capping tomes on all your characters.",
            StopNotificationType.RunnerStopped => "The runner has finished a duty tomestone run and has stopped.",
            _ => throw new ArgumentOutOfRangeException(nameof(stopNotificationType), stopNotificationType, null)
        };
    }
}
