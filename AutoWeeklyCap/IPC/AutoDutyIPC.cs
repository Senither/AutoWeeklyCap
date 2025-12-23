using System;
using ECommons.EzIpcManager;

namespace AutoWeeklyCap.IPC;

public class AutoDutyIPC
{
    public static EzIPCDisposalToken[] disposalTokens =
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
