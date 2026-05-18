using ECommons.EzIpcManager;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

namespace AutoWeeklyCap.IPC;

public class StylistIPC
{
    internal const string Name = "Stylist";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static readonly EzIPCDisposalToken[] DisposalTokens =
        EzIPC.Init(typeof(StylistIPC), Name, SafeWrapper.IPCException);

    internal static readonly PluginInstallerHelper.PluginContext Context = new(
        Name,
        displayName: "Stylist",
        description: "Gear manager, used to equip gear upgrades as you level your characters.",
        repositoryUrl: "https://github.com/NightmareXIV/Stylist"
    );

    [EzIPC] internal static Func<bool> IsBusy;

    /// <summary>
    /// Updates current gearset, if present
    /// </summary>
    /// <param name="moveItemsFromInventory">null - respect configuration choice</param>
    /// <param name="shouldEquip">Whether to equip specified gearset. Setting it to true will always equip it, no matter if it was updated or not. Setting it to false will never equip it. Setting it to null will use player's preferences.</param>
    [EzIPC] internal static Action<bool?, bool?> UpdateCurrentGearsetEx;

    internal static void Dispose()
    {
        IPCSubscriber.DisposeAll(DisposalTokens);
    }
}

#pragma warning restore CS8618
#pragma warning restore CS0649
