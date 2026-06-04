namespace AutoWeeklyCap.Enums;

public enum GearProfile
{
    Minimal,
    Limited,
    Partial,
    Full
}

public static class GearProfileExtensions
{
    extension(GearProfile profile)
    {
        public string GetName()
        {
            return profile switch
            {
                GearProfile.Minimal => "Minimal",
                GearProfile.Limited => "Limited",
                GearProfile.Partial => "Partial",
                GearProfile.Full => "Fully Geared",
                _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null)
            };
        }

        public string GetDescription()
        {
            return profile switch
            {
                GearProfile.Minimal => "Gear for for the first dungeon",
                GearProfile.Limited => "Gear for accessing first two dungeons",
                GearProfile.Partial => "Gears for the first three dungeons",
                GearProfile.Full => "Buys gear for all leveling dungeons of expansion",
                _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null)
            };
        }

        public int GetSubtractedItemLevel()
        {
            return profile switch
            {
                GearProfile.Minimal => 15,
                GearProfile.Limited => 10,
                GearProfile.Partial => 5,
                GearProfile.Full => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null)
            };
        }
    }
}
