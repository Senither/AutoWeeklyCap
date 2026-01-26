namespace AutoWeeklyCap.IPC;

public static class BossModReborn
{
    internal const string Name = "BossModReborn";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);
}
