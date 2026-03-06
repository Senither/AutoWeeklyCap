using ECommons.EzIpcManager;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

namespace AutoWeeklyCap.IPC;

public class AutoDutyIPC
{
    internal const string Name = "AutoDuty";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static readonly EzIPCDisposalToken[] DisposalTokens =
        EzIPC.Init(typeof(AutoDutyIPC), Name, SafeWrapper.IPCException);

    internal static readonly PluginInstallerHelper.PluginContext Context = new(
        Name,
        "Used to run the duties when farming tomestones.",
        repositoryUrl: "https://github.com/erdelf/AutoDuty"
    );

    [EzIPC] internal static Action<uint, int, bool> Run;
    [EzIPC] internal static Action Stop;
    [EzIPC] internal static Func<bool> IsStopped;

    internal static void Dispose()
    {
        IPCSubscriber.DisposeAll(DisposalTokens);
    }
}

#pragma warning restore CS8618
#pragma warning restore CS0649
