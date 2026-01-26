namespace AutoWeeklyCap.IPC;

public class RotationSolverReborn
{
    internal const string Name = "RotationSolver";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);
}
