using System;
using ECommons.EzIpcManager;

namespace AutoWeeklyCap.IPC;

public class AutoDutyIPC
{
    public static EzIPCDisposalToken[] disposalTokens =
        EzIPC.Init(typeof(AutoDutyIPC), "AutoDuty", SafeWrapper.IPCException);

    internal static bool IsEnabled => IPCSubscriber.IsReady("AutoDuty");

    [EzIPC]
    internal static Action Run;

    [EzIPC]
    internal static Action<bool> Start;

    internal static void Dispose() => IPCSubscriber.DisposeAll(disposalTokens);
}
