namespace AutoWeeklyCap.IPC;

public static class NoKillPlugin
{
    internal static bool IsEnabled => IPCSubscriber.IsReady("NoKillPlugin");
}
