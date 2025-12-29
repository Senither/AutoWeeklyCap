using System;
using ECommons.EzIpcManager;

namespace AutoWeeklyCap.IPC;

// ReSharper disable InconsistentNaming

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

public class AutoDutyIPC
{
    public static readonly EzIPCDisposalToken[] disposalTokens =
        EzIPC.Init(typeof(AutoDutyIPC), "AutoDuty", SafeWrapper.IPCException);

    internal static bool IsEnabled => IPCSubscriber.IsReady("AutoDuty");

    [EzIPC]
    internal static Action<uint, int, bool> Run;

    [EzIPC]
    internal static Action Stop;

    [EzIPC]
    internal static Func<bool> IsStopped;

    internal static void Dispose() => IPCSubscriber.DisposeAll(disposalTokens);
}

#pragma warning restore CS8618
#pragma warning restore CS0649
