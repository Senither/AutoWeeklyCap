using AutoWeeklyCap.IPC.Wotsit;

using ECommons.EzIpcManager;

namespace AutoWeeklyCap.IPC;

public static class IPCSubscriber
{
    internal static bool IsReady(string pluginName)
    {
        return AWC.PluginInterface.InstalledPlugins.FirstOrDefault(plugin => plugin.InternalName == pluginName && plugin.IsLoaded) != null;
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
        GenericHelpers.Safe(StylistIPC.Dispose);
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
