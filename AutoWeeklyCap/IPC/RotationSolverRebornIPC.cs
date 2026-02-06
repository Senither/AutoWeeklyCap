// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.IPC;

public static class RotationSolverRebornIPC
{
    internal const string Name = "RotationSolver";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);
}
