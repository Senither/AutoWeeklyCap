namespace AutoWeeklyCap.Runner.Zone;

public static class DutyZone
{
    public static uint GetZoneId(bool leveling)
    {
        return (leveling)
            ? LevelZone.GetZoneId()
            : TomestoneZone.GetZoneId();
    }
}
