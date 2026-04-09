using ECommons.EzIpcManager;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

namespace AutoWeeklyCap.IPC;

public static class Delegates
{
    public delegate bool PathfindAndMoveTo(Vector3 position, bool canFly);
}

public class VNavMeshIPC
{
    internal const string Name = "vnavmesh";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static readonly EzIPCDisposalToken[] DisposalTokens =
        EzIPC.Init(typeof(VNavMeshIPC), Name, SafeWrapper.IPCException);

    internal static readonly PluginInstallerHelper.PluginContext Context = new(
        Name,
        "Handles navigating within a zone, moving your character to retainer bells and NPCs for repairs or buying materials.",
        repositoryUrl: "https://github.com/awgil/ffxiv_navmesh"
    );

    [EzIPC("Nav.IsReady")] internal static Func<bool> IsReady;
    [EzIPC("Nav.Rebuild")] internal static Func<bool> Rebuild;
    [EzIPC("Nav.PathfindInProgress")] internal static Func<bool> PathfindInProgress;

    /// <summary>
    /// Vector3 position, bool canFly
    /// </summary>
    [EzIPC("SimpleMove.PathfindAndMoveTo")]
    internal static Delegates.PathfindAndMoveTo PathfindAndMoveTo;

    [EzIPC("Path.Stop")] internal static Action Stop;
    [EzIPC("Path.IsRunning")] internal static Func<bool> IsRunning;
    [EzIPC("Path.SetAlignCamera")] internal static Action<bool> SetAlignCamera;
    [EzIPC("Path.SetTolerance")] internal static Action<float> SetTolerance;

    internal static void Dispose()
    {
        IPCSubscriber.DisposeAll(DisposalTokens);
    }
}

#pragma warning restore CS8618
#pragma warning restore CS0649
