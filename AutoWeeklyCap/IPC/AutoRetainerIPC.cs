using AutoWeeklyCap.IPC.AutoRetainer;
using ECommons.EzIpcManager;

// ReSharper disable InconsistentNaming
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

namespace AutoWeeklyCap.IPC;

public class AutoRetainerIPC
{
    internal const string Name = "AutoRetainer";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static readonly EzIPCDisposalToken[] disposalTokens =
        EzIPC.Init(typeof(AutoRetainerIPC), $"{Name}.PluginState", SafeWrapper.IPCException);

    internal static readonly PluginInstallerHelper.PluginContext Context = new(
        pluginName: Name,
        description: "Used to mange retainer ventures and deployables on all your characters.",
        websiteUrl: "https://puni.sh/",
        repositoryUrl: "https://github.com/PunishXIV/AutoRetainer"
    );

    internal static void EnableMultiMode()
    {
        if (IsEnabled && !GetMultiModeStatus())
            ChatHelper.RunCommand("autoretainer multi enable");
    }

    internal static void DisableMultiMode()
    {
        if (IsEnabled && GetMultiModeStatus())
            ChatHelper.RunCommand("autoretainer multi disable");
    }

    [EzIPC]
    internal static Func<bool> IsBusy;

    [EzIPC]
    internal static Func<bool> GetMultiModeStatus;

    [EzIPC]
    internal static Func<ulong, long?> GetClosestRetainerVentureSecondsRemaining;

    internal static List<ulong> GetRegisteredCharacters()
        => Svc.PluginInterface.GetIpcSubscriber<List<ulong>>("AutoRetainer.GetRegisteredCIDs").InvokeFunc();

    internal static OfflineCharacterData GetOfflineCharacterData(ulong cid)
        => Svc.PluginInterface.GetIpcSubscriber<ulong, OfflineCharacterData>("AutoRetainer.GetOfflineCharacterData").InvokeFunc(cid);

    internal static void Dispose() => IPCSubscriber.DisposeAll(disposalTokens);
}

#pragma warning restore CS8618
#pragma warning restore CS0649
