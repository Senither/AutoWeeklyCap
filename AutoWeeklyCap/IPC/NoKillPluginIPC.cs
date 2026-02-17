// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.IPC;

public static class NoKillPluginIPC
{
    internal const string Name = "NoKillPlugin";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static readonly PluginInstallerHelper.PluginContext Context = new(
        pluginName: Name,
        description: "Prevents the game from closing when getting lobby errors (Prolonged network issues)",
        repositoryUrl: "https://github.com/Bluefissure/NoKillPlugin",
        nativeDalamudPlugin: true
    );
}
