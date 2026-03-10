using AutoWeeklyCap.IPC.Wotsit;

using ECommons.EzIpcManager;
using ECommons.Reflection;

namespace AutoWeeklyCap.IPC;

public static class IPCSubscriber
{
    internal static bool IsReady(string pluginName)
    {
        return DalamudReflector.TryGetDalamudPlugin(pluginName, out _, true, true);
    }

    internal static void Dispose()
    {
        GenericHelpers.Safe(AutoDutyIPC.Dispose);
        GenericHelpers.Safe(AutoRetainerIPC.Dispose);
        GenericHelpers.Safe(DeliverooIPC.Dispose);
        GenericHelpers.Safe(LifestreamIPC.Dispose);
        GenericHelpers.Safe(NotificationMasterIPC.Dispose);
        GenericHelpers.Safe(VNavMeshIPC.Dispose);
        GenericHelpers.Safe(WotsitIPC.Dispose);
    }

    internal static void DisposeAll(EzIPCDisposalToken[] disposalTokens)
    {
        foreach (var token in disposalTokens) {
            try {
                token.Dispose();
            } catch (Exception ex) {
                AWC.Log.Error($"Error while unregistering IPC: {ex}");
            }
        }
    }
}
