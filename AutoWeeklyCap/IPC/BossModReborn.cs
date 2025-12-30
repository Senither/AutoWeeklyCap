namespace AutoWeeklyCap.IPC;

public static class BossModReborn
{
    internal static bool IsEnabled => IPCSubscriber.IsReady("BossModReborn");
}
