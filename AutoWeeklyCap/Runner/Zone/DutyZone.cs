using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Runner.Zone;

public static class DutyZone
{
    public static uint GetZoneId(bool leveling)
    {
        return (leveling)
            ? LevelZone.GetZoneId()
            : TomestoneZone.GetZoneId();
    }

    public static uint GetRequiredItemLevel(uint zoneId)
    {
        var contentFinderConditions = Svc.Data.GameData.GetExcelSheet<ContentFinderCondition>();

        var condition = contentFinderConditions?.FirstOrNull(content => content.TerritoryType.ValueNullable?.RowId == zoneId);
        if (condition == null) {
            return 0;
        }

        return condition.Value.ItemLevelRequired;
    }
}
