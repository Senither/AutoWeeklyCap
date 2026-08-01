using AutoWeeklyCap.IPC.Lifestream;

using ECommons.EzIpcManager;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

namespace AutoWeeklyCap.IPC;

public class LifestreamIPC
{
    internal const string Name = "Lifestream";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static readonly EzIPCDisposalToken[] DisposalTokens =
        EzIPC.Init(typeof(LifestreamIPC), Name, SafeWrapper.IPCException);

    internal static readonly PluginInstallerHelper.PluginContext Context = new(
        Name,
        "Used to travel to aethernet shards in cities, and switch between characters.",
        repositoryUrl: "https://github.com/NightmareXIV/Lifestream"
    );

    [EzIPC] internal static Func<bool> IsBusy;
    [EzIPC] internal static Func<string, string, ErrorCode> ChangeCharacter;
    [EzIPC] internal static Action<string> ExecuteCommand;
    [EzIPC] internal static Func<bool> HasApartment;
    [EzIPC] internal static Func<bool> HasPrivateHouse;
    [EzIPC] internal static Func<bool> HasFreeCompanyHouse;
    [EzIPC] internal static Func<(int Kind, int Ward, int Plot)?> GetCurrentPlotInfo;
    [EzIPC] internal static Func<ulong, (HousePathData? Private, HousePathData? FC)> GetHousePathData;
    [EzIPC] internal static Action Abort;
    [EzIPC] internal static Func<ErrorCode> Logout;

    internal static void Dispose()
    {
        IPCSubscriber.DisposeAll(DisposalTokens);
    }
}

#pragma warning restore CS8618
#pragma warning restore CS0649
