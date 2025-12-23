using System;
using ECommons;
using ECommons.EzIpcManager;

namespace AutoWeeklyCap.IPC;

public class LifestreamIPC
{
    public static EzIPCDisposalToken[] disposalTokens =
        EzIPC.Init(typeof(LifestreamIPC), "Lifestream", SafeWrapper.IPCException);

    internal static bool IsEnabled => IPCSubscriber.IsReady("Lifestream");

    [EzIPC]
    internal static Func<bool> IsBusy;

    [EzIPC]
    internal static Func<string, string, ErrorCode> ChangeCharacter;

    internal static void Dispose() => IPCSubscriber.DisposeAll(disposalTokens);
}
