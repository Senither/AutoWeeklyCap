namespace AutoWeeklyCap.IPC;

public class SkipCutsceneIPC
{
    internal const string Name = "SkipCutscene";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static readonly PluginInstallerHelper.PluginContext Context = new(
        Name,
        displayName: "Skip Cutscene",
        description: "Helps speed up leveling by skipping cut screens in MSQ dungeons.",
        repositoryUrl: "https://github.com/KangasZ/SkipCutscene"
    );
}
