using ECommons.EzIpcManager;

// ReSharper disable InconsistentNaming
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

namespace AutoWeeklyCap.IPC;

public static class DeliverooIPC
{
    internal const string Name = "Deliveroo";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static readonly EzIPCDisposalToken[] disposalTokens =
        EzIPC.Init(typeof(DeliverooIPC), Name, SafeWrapper.IPCException);

    internal static readonly PluginInstallerHelper.PluginContext Context = new(
        Name,
        "Used to automate your grand company deliveries to get GC seals, and spend them to buy your preferred items.",
        repositoryUrl: "https://github.com/VeraNala/Deliveroo"
    );

    internal static void StartTurnIn()
    {
        if (!IsTurnInRunning()) {
            ChatHelper.RunCommand("deliveroo enable");
        }
    }

    internal static void StopTurnIn()
    {
        if (IsTurnInRunning()) {
            ChatHelper.RunCommand("deliveroo disable");
        }
    }

    [EzIPC] internal static Func<bool> IsTurnInRunning;

    internal static void Dispose()
    {
        IPCSubscriber.DisposeAll(disposalTokens);
    }
}

#pragma warning restore CS8618
#pragma warning restore CS0649
