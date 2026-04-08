using System.Diagnostics.CodeAnalysis;

using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Helpers;

public static unsafe class HousingHelper
{
    public static bool IsInsideHouse()
    {
        return IsInsideOwnedEstate(false, EstateType.PersonalEstate, EstateType.SharedEstate);
    }

    public static bool IsInsideApartment()
    {
        var hm = HousingManager.Instance();
        if (hm == null || !hm->IsInside()) {
            return false;
        }

        var currentHouseId = hm->GetCurrentHouseId();
        if (!currentHouseId.IsApartment) {
            return false;
        }

        return MatchesEstate(currentHouseId, HousingManager.GetOwnedHouseId(EstateType.ApartmentBuilding)) ||
               MatchesEstate(currentHouseId, HousingManager.GetOwnedHouseId(EstateType.ApartmentRoom));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static bool IsInsideFC()
    {
        return IsInsideOwnedEstate(true, EstateType.FreeCompanyEstate, EstateType.PersonalChambers);
    }

    private static bool IsInsideOwnedEstate(bool allowWorkshop, params EstateType[] estateTypes)
    {
        var hm = HousingManager.Instance();
        if (hm == null || (!hm->IsInside() && (!allowWorkshop || !hm->IsInWorkshop()))) {
            return false;
        }

        var currentHouseId = hm->GetCurrentHouseId();
        foreach (var estateType in estateTypes) {
            if (estateType == EstateType.SharedEstate) {
                for (var i = 0; i < 2; i++) {
                    if (MatchesEstate(currentHouseId, HousingManager.GetOwnedHouseId(estateType, i))) {
                        return true;
                    }
                }

                continue;
            }

            if (MatchesEstate(currentHouseId, HousingManager.GetOwnedHouseId(estateType))) {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesEstate(HouseId currentHouseId, HouseId ownedHouseId)
    {
        return ownedHouseId.Id != 0 &&
               currentHouseId.WorldId == ownedHouseId.WorldId &&
               currentHouseId.TerritoryTypeId == ownedHouseId.TerritoryTypeId &&
               currentHouseId.WardIndex == ownedHouseId.WardIndex &&
               currentHouseId.Unit == ownedHouseId.Unit;
    }
}
