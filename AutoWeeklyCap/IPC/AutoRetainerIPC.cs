using System;
using AutoWeeklyCap.Helpers;
using ECommons.EzIpcManager;

namespace AutoWeeklyCap.IPC;

// ReSharper disable InconsistentNaming
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

public class AutoRetainerIPC
{
    public static readonly EzIPCDisposalToken[] disposalTokens =
        EzIPC.Init(typeof(AutoRetainerIPC), "AutoRetainer.PluginState", SafeWrapper.IPCException);

    internal static bool IsEnabled => IPCSubscriber.IsReady("AutoRetainer");

    internal static void EnableMultiMode()
    {
        if (IsEnabled && !GetMultiModeStatus())
            Chat.RunCommand("autoretainer multi enable");
    }
    
    internal static void DisableMultiMode()
    {
        if (IsEnabled && GetMultiModeStatus())
            Chat.RunCommand("autoretainer multi disable");
    }

    [EzIPC]
    internal static Func<bool> IsBusy;

    [EzIPC]
    internal static Func<bool> GetMultiModeStatus;

    [EzIPC]
    internal static Func<ulong, long?> GetClosestRetainerVentureSecondsRemaining;

    internal static void Dispose() => IPCSubscriber.DisposeAll(disposalTokens);
}

#pragma warning restore CS8618
#pragma warning restore CS0649
