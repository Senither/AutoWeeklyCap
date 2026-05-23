using FFXIVClientStructs.FFXIV.Client.Game.UI;

using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Runner.Zone;

public static class TomestoneZone
{
    public static readonly uint[] AvailableTomestoneZones =
    [
        1345, // The Clyteum
        1314, // Mistwake
        1292, // The Meso Terminal
        1266, // The Underkeep
        1242, // Yuweyawata Field Station
        1199 // Alexandria
    ];

    private static readonly Dictionary<uint, uint> Contents = new();

    public static uint GetZoneId()
    {
        if (!Player.Available) {
            return AWC.Config.ZoneId;
        }

        var contentZones = GetTomestoneContentZones();
        var currentItemLevel = InventoryHelper.GetCurrentItemLevel();

        var selectedZone = contentZones.FirstOrNull(content => content.Key == AWC.Config.ZoneId);
        if (selectedZone != null && CanEnterTomestoneZone((KeyValuePair<uint, uint>)selectedZone, currentItemLevel)) {
            return AWC.Config.ZoneId;
        }

        foreach (var zone in GetTomestoneContentZones()) {
            if (CanEnterTomestoneZone(zone, currentItemLevel)) {
                return zone.Key;
            }
        }

        return 0;
    }

    public static bool IsSupportedTomestoneZone(uint zoneId)
    {
        return AvailableTomestoneZones.Contains(zoneId);
    }

    private static bool CanEnterTomestoneZone(KeyValuePair<uint, uint> zone, int itemLevel)
    {
        return UIState.IsInstanceContentUnlocked(zone.Value)
               && DutyZone.GetRequiredItemLevel(zone.Key) <= itemLevel;
    }

    private static Dictionary<uint, uint> GetTomestoneContentZones()
    {
        if (Contents.Count > 0) {
            return Contents;
        }

        var contentFinderConditions = Svc.Data.GameData.GetExcelSheet<ContentFinderCondition>();
        if (contentFinderConditions == null) {
            return Contents;
        }

        foreach (var zone in AvailableTomestoneZones) {
            var condition = contentFinderConditions.FirstOrNull(condition => condition.TerritoryType.ValueNullable?.RowId == zone);
            if (condition == null) {
                continue;
            }

            Contents.Add(zone, condition.Value.Content.RowId);
        }

        return Contents;
    }
}
