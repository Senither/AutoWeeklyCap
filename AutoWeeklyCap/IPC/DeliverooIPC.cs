using System;
using AutoWeeklyCap.Helpers;
using ECommons.EzIpcManager;

namespace AutoWeeklyCap.IPC;

public static class DeliverooIPC
{
    internal const string Name = "Deliveroo";

    public static readonly EzIPCDisposalToken[] disposalTokens =
        EzIPC.Init(typeof(DeliverooIPC), Name, SafeWrapper.IPCException);

    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static void StartTurnIn()
    {
        if (!IsTurnInRunning())
            ChatHelper.RunCommand("deliveroo enable");
    }

    internal static void StopTurnIn()
    {
        if (IsTurnInRunning())
            ChatHelper.RunCommand("deliveroo disable");
    }

    [EzIPC]
    internal static Func<bool> IsTurnInRunning { get; private set; }

    internal static void Dispose() => IPCSubscriber.DisposeAll(disposalTokens);
}
