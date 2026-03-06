using ECommons.EzIpcManager;

// ReSharper disable InconsistentNaming
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null

namespace AutoWeeklyCap.IPC;

public static class NotificationMasterIPC
{
    internal const string Name = "NotificationMaster";
    internal static bool IsEnabled => IPCSubscriber.IsReady(Name);

    internal static readonly EzIPCDisposalToken[] disposalTokens =
        EzIPC.Init(typeof(NotificationMasterIPC), $"{Name}API", SafeWrapper.IPCException);

    internal static readonly PluginInstallerHelper.PluginContext Context = new(
        Name,
        "Used to send notifications outside the game to notify you when the runner is done, such as making the game icon in the taskbar flash, sending toast notifications, and playing audio.",
        repositoryUrl: "https://github.com/NightmareXIV/NotificationMaster",
        nativeDalamudPlugin: true
    );

    [EzIPC] private static Func<string, bool> FlashTaskbarIcon;

    internal static bool SendFlashTaskbarIcon()
    {
        return IsEnabled && FlashTaskbarIcon(AWC.InternalName);
    }

    [EzIPC] private static Func<string, string, string, bool> DisplayToastNotification;

    internal static bool SendDisplayToastNotification(string title, string content)
    {
        return IsEnabled && DisplayToastNotification(AWC.InternalName, title, content);
    }

    [EzIPC] private static Func<string, string, float, bool, bool, bool> PlaySound;

    internal static bool SendPlaySound(string path, float volume, bool repeat, bool stopOnceFocused)
    {
        return IsEnabled && PlaySound(AWC.InternalName, path, volume, repeat, stopOnceFocused);
    }

    [EzIPC] private static Func<string, bool> StopSound;

    internal static bool SendStopSound()
    {
        return IsEnabled && StopSound(AWC.InternalName);
    }

    internal static void Dispose()
    {
        IPCSubscriber.DisposeAll(disposalTokens);
    }
}

#pragma warning restore CS8618
#pragma warning restore CS0649
