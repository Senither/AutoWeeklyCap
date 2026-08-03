using Dalamud.Game.Config;

namespace AutoWeeklyCap.Helpers;

public static class AudioHelper
{
    internal static void MuteMasterGameAudio(bool mute = true)
    {
        AWC.Log.Debug($"AudioHelper: Update SndMaster mute status to {mute}");

        Svc.GameConfig.Set(SystemConfigOption.IsSndMaster, mute ? 1u : 0u);
    }
}
