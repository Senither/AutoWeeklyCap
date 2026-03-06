namespace AutoWeeklyCap.Helpers;

public static class AutoRetainerHelper
{
    public static bool HasRetainerWithinThreshold()
    {
        if (!AutoRetainerIPC.IsEnabled) {
            return false;
        }

        if (!PlayerHelper.IsValid) {
            return false;
        }

        var seconds = AutoRetainerIPC.GetClosestRetainerVentureSecondsRemaining(Player.CID);
        if (!seconds.HasValue) {
            return false;
        }

        return seconds.Value <= AWC.Config.AutoRetainerThreshold;
    }
}
