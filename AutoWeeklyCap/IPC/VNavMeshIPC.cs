// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using ECommons.EzIpcManager;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

namespace AutoWeeklyCap.IPC;

public class VNavMeshIPC
{
    internal const string Name = "vnavmesh";

    internal static readonly EzIPCDisposalToken[] disposalTokens =
        EzIPC.Init(typeof(VNavMeshIPC), Name, SafeWrapper.IPCException);

    public static class Delegates
    {
        public delegate Task<List<Vector3>> Pathfind(Vector3 from, Vector3 to, bool isFlying);

        public delegate bool PathfindAndMoveTo(Vector3 position, bool canFly);

        public delegate void PathMoveTo(List<Vector3> waypoints, bool fly);
    }

    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    [EzIPC("Nav.IsReady")]
    internal static Func<bool> IsReady { get; private set; }

    [EzIPC("Nav.BuildProgress")]
    internal static Func<float> BuildProgress { get; private set; }

    [EzIPC("Nav.Reload")]
    internal static Func<bool> Reload { get; private set; }

    [EzIPC("Nav.Rebuild")]
    internal static Func<bool> Rebuild { get; private set; }

    /// <summary>
    /// Vector3 from, Vector3 to, bool fly
    /// </summary>
    [EzIPC("Nav.Pathfind")]
    internal static Delegates.Pathfind Pathfind { get; private set; }

    /// <summary>
    /// Vector3 position, bool canFly
    /// </summary>
    [EzIPC("SimpleMove.PathfindAndMoveTo")]
    internal static Delegates.PathfindAndMoveTo PathfindAndMoveTo { get; private set; }

    [EzIPC("SimpleMove.PathfindInProgress")]
    internal static Func<bool> PathfindInProgress { get; private set; }

    [EzIPC("Path.Stop")]
    internal static Action Stop { get; private set; }

    [EzIPC("Path.IsRunning")]
    internal static Func<bool> IsRunning { get; private set; }

    /// <summary>
    /// Vector3 p, float halfExtentXZ, float halfExtentY
    /// </summary>
    [EzIPC("Query.Mesh.NearestPoint")]
    internal static Func<Vector3, float, float, Vector3?> NearestPoint { get; private set; }

    [EzIPC("Path.MoveTo")]
    internal static Delegates.PathMoveTo MoveTo { get; private set; }

    [EzIPC("Path.NumWaypoints")]
    internal static Func<int> NumWaypoints { get; private set; }

    [EzIPC("Path.GetMovementAllowed")]
    internal static Func<bool> GetMovementAllowed { get; private set; }

    [EzIPC("Path.SetMovementAllowed")]
    internal static Action<bool> SetMovementAllowed { get; private set; }

    [EzIPC("Path.GetAlignCamera")]
    internal static Func<bool> GetAlignCamera { get; private set; }

    [EzIPC("Path.SetAlignCamera")]
    internal static Action<bool> SetAlignCamera { get; private set; }

    [EzIPC("Path.GetTolerance")]
    internal static Func<float> GetTolerance { get; private set; }

    [EzIPC("Path.SetTolerance")]
    internal static Action<float> SetTolerance { get; private set; }

    internal static void Dispose() => IPCSubscriber.DisposeAll(disposalTokens);
}

#pragma warning restore CS8618
#pragma warning restore CS0649
