using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Helpers;

public static class MapHelper
{
    public static string? GetZoneNameFromId(uint zoneId)
    {
        if (AutoWeeklyCap.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(zoneId, out var territoryRow))
        {
            var name = territoryRow.PlaceName.Value.Name.ExtractText();

            return name.Length == 0 ? null : name;
        }

        return null;
    }
}
