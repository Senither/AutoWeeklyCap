namespace AutoWeeklyCap.Runner;

public static class LocationManager
{
    private static Safezone? LastKnownLocation { get; set; } = null;

    public static void RegisterLocation()
    {
        if (HousingHelper.IsInsideHouse()) {
            LastKnownLocation = Safezone.PrivateHouse;
        } else if (HousingHelper.IsInsideFC()) {
            LastKnownLocation = Safezone.FreeCompany;
        } else {
            LastKnownLocation = null;
        }
    }

    public static Safezone? GetLastKnownLocation()
    {
        return LastKnownLocation;
    }

    public static void Reset()
    {
        LastKnownLocation = null;
    }
}
