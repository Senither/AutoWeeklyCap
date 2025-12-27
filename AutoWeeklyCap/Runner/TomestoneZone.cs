using System;

namespace AutoWeeklyCap.Runner;

public class TomestoneZone
{
    public static readonly uint[] AvailableTomestoneZones =
    {
        1314, // Mistwake
        1292, // The Meso Terminal
        1266, // The Underkeep
        1242, // Yuweyawata Field Station
        1199, // Alexandria
    };

    public static bool IsSupportedTomestoneZone(uint zoneId)
    {
        return AvailableTomestoneZones.Contains(zoneId);
    }
}
