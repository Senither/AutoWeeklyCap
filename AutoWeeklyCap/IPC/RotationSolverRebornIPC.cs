// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.IPC;

public static class RotationSolverRebornIPC
{
    internal const string Name = "RotationSolver";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static readonly PluginInstallerHelper.PluginContext Context = new(
        pluginName: Name,
        displayName: "Rotation Solver Reborn",
        description: "Better combat rotation solver, making duty runs quicker and more seamless.",
        repositoryUrl: "https://github.com/FFXIV-CombatReborn/RotationSolverReborn"
    );
}
