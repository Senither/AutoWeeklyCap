using Dalamud.Game.Config;

namespace AutoWeeklyCap.Helpers;

public static class SystemConfigHelper
{
    internal static void MuteMasterGameAudio(bool mute = true)
    {
        AWC.Log.Debug($"SystemConfigHelper: Update SndMaster mute status to {mute}");

        Svc.GameConfig.Set(SystemConfigOption.IsSndMaster, mute ? 1u : 0u);
    }

    internal static void LimitFrameRateWhenClientIsInactive(bool enabled = true)
    {
        Svc.GameConfig.Set(SystemConfigOption.FPSInActive, enabled ? 1u : 0u);
    }

    internal static bool GetLimitedFrameRateWhenClientIsInactive()
    {
        if (Svc.GameConfig.TryGet(SystemConfigOption.FPSInActive, out uint value)) {
            return value == 1u;
        }

        return false;
    }
}
