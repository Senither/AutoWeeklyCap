// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.IPC;

public static class NoKillPluginIPC
{
    internal const string Name = "NoKillPlugin";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);
}
