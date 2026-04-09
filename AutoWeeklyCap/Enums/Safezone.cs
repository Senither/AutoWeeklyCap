using AutoWeeklyCap.Runner.Actions;

namespace AutoWeeklyCap.Enums;

public enum Safezone
{
    PrivateHouse,
    FreeCompany,
    Apartment,
    GrandCompanyInn,
}

public static class SafezoneExtensions
{
    extension(Safezone safezone)
    {
        public string GetName()
        {
            return safezone switch
            {
                Safezone.PrivateHouse => "Private House",
                Safezone.FreeCompany => "Free Company",
                Safezone.Apartment => "Apartment",
                Safezone.GrandCompanyInn => "Grand Company Inn",
                _ => throw new ArgumentOutOfRangeException(nameof(safezone), safezone, null)
            };
        }

        public bool Invoke()
        {
            return safezone switch
            {
                Safezone.PrivateHouse => ActionInstance.EnterPrivateHouse.Invoke(),
                Safezone.FreeCompany => ActionInstance.EnterFcHouseAction.Invoke(),
                Safezone.Apartment => ActionInstance.EnterApartmentAction.Invoke(),
                Safezone.GrandCompanyInn => ActionInstance.EnterGrandCompanyInn.Invoke(),
                _ => throw new ArgumentOutOfRangeException(nameof(safezone), safezone, null)
            };
        }

        public bool IsOnLocation()
        {
            return safezone switch
            {
                Safezone.PrivateHouse => HousingHelper.IsInsideHouse(),
                Safezone.FreeCompany => HousingHelper.IsInsideFC(),
                Safezone.Apartment => HousingHelper.IsInsideApartment(),
                Safezone.GrandCompanyInn => Player.Available && Player.Territory.RowId == GrandCompanyHelper.InnTerritoryId,
                _ => throw new ArgumentOutOfRangeException(nameof(safezone), safezone, null)
            };
        }
    }
}
