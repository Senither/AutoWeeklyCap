namespace AutoWeeklyCap.IPC.Wotsit;

public static class WotsitIPC
{
    internal const string Name = "Dalamud.FindAnything";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static readonly PluginInstallerHelper.PluginContext Context = new(
        pluginName: Name,
        description: "Adds a spotlight like quick search, can be used to start and stop the runner, or run individual features.",
        repositoryUrl: "https://github.com/goaaats/Dalamud.FindAnything",
        nativeDalamudPlugin: true
    );

    internal static readonly WotsitManager Manager = new();

    internal static void Dispose() => Manager.Dispose();
}
