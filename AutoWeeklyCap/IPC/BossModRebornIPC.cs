// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.IPC;

public static class BossModRebornIPC
{
    internal const string Name = "BossModReborn";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static readonly PluginInstallerHelper.PluginContext Context = new(
        pluginName: Name,
        description: "Better combat AI for dodging and avoiding attacks while in duties.",
        repositoryUrl: "https://github.com/FFXIV-CombatReborn/BossmodReborn"
    );
}
