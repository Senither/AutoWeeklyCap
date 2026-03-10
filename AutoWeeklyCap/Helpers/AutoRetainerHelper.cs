namespace AutoWeeklyCap.Helpers;

public static class AutoRetainerHelper
{
    public static bool HasRetainerWithinThreshold()
    {
        if (!AutoRetainerIPC.IsEnabled) {
            return false;
        }

        foreach (var option in AWC.Config.Characters.Values) {
            var seconds = AutoRetainerIPC.GetClosestRetainerVentureSecondsRemaining(option.ID);
            if (!seconds.HasValue) {
                continue;
            }

            if (seconds.Value <= AWC.Config.AutoRetainerThreshold) {
                return true;
            }
        }

        return false;
    }
}
