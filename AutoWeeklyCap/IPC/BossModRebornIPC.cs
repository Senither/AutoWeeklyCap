// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.IPC;

public static class BossModRebornIPC
{
    internal const string Name = "BossModReborn";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);
}
