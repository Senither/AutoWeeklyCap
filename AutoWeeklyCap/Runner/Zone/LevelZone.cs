using FFXIVClientStructs.FFXIV.Client.Game.UI;

using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Runner.Zone;

public record LevelZoneRecord(int Level, uint ZoneId, Func<bool>? Condition = null);

public static class LevelZone
{
    private static readonly List<LevelZoneRecord> AvailableLevelingZones =
    [
        new(15, 1036), // Sastasha
        new(16, 1037), // The Tam'Tara Deepcroft
        new(24, 1039), // The Thousand Maws of Toto-Rak
        new(32, 1041), // Brayflox's Longstop
        new(38, 1303), // Cutter's Cry
        new(41, 1042), // Stone Vigil
        new(44, 1330), // Dzemael Darkhold
        new(47, 1331), // Aurum Vale
        new(50, 1044, () => SkipCutsceneIPC.IsEnabled), // The Praetorium
        new(51, 1366, () => !SkipCutsceneIPC.IsEnabled), // The Dusk Vigil
        new(53, 1064, () => !SkipCutsceneIPC.IsEnabled), // Sohm Al
        new(55, 1065), // The Aery
        new(57, 1066), // The Vault
        new(59, 1109), // The Great Gubal Library
        new(61, 1142), // Sirensong Sea
        new(67, 1144), // Doma Castle
        new(69, 1145), // Castrum Abania
        new(71, 837), // Holminster
        new(73, 821), // Dohn Mheg
        new(75, 823), // Qitana
        new(77, 836), // Malikah's Well
        new(79, 822), // Mt. Gulg
        new(81, 952), // Tower of Zot
        new(83, 969), // Tower of Babil
        new(85, 970), // Vanaspati,
        new(87, 974), // Ktisis Hyperboreia
        new(89, 978), // Aitiascope
        new(91, 1167), // Ihuykatumu
        new(93, 1193), // Worqor Zormor
        new(95, 1194), // The Skydeep Cenote
        new(97, 1198), // Vanguard
        new(99, 1208), // Origenics
    ];

    private static readonly Dictionary<uint, uint> Contents = new();

    public static uint GetZoneId()
    {
        if (!Player.Available) {
            return AWC.Config.ZoneId;
        }

        var currentJob = (PlayerJob)AWC.PlayerState.ClassJob.RowId;
        var currentJobLevel = PlayerHelper.GetJobLevel(currentJob);

        foreach (var zone in GetEligibleZonesForLevel(currentJobLevel)) {
            if (UIState.IsInstanceContentUnlocked(zone.contentId)) {
                return zone.zone.ZoneId;
            }
        }

        return 0;
    }

    private static List<(LevelZoneRecord zone, uint contentId)> GetEligibleZonesForLevel(int level)
    {
        var contentZones = GetLevelingContentZones();
        var currentItemLevel = InventoryHelper.GetCurrentItemLevel();

        return AvailableLevelingZones
            .Where(zone => zone.Level <= level)
            .Where(zone => zone.Condition?.Invoke() ?? true)
            .Where(zone => DutyZone.GetRequiredItemLevel(zone.ZoneId) <= currentItemLevel)
            .OrderByDescending(zone => zone.Level)
            .Select(zone =>
            {
                contentZones.TryGetValue(zone.ZoneId, out var contentId);
                return (zone, contentId);
            })
            .Where(entry => entry.contentId != 0)
            .ToList();
    }

    private static Dictionary<uint, uint> GetLevelingContentZones()
    {
        if (Contents.Count > 0) {
            return Contents;
        }

        var contentFinderConditions = Svc.Data.GameData.GetExcelSheet<ContentFinderCondition>();
        if (contentFinderConditions == null) {
            return Contents;
        }

        foreach (var zone in AvailableLevelingZones) {
            var condition = contentFinderConditions.FirstOrNull(content => content.TerritoryType.ValueNullable?.RowId == zone.ZoneId);
            if (condition == null) {
                continue;
            }

            Contents[zone.ZoneId] = condition.Value.Content.RowId;
        }

        return Contents;
    }
}
