using AutoWeeklyCap.Actions;
using AutoWeeklyCap.Runner;
using Dalamud.Configuration;

namespace AutoWeeklyCap.Config;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    // General Options
    public bool OpenWindowOnStartup { get; set; } = false;
    public bool UseSliders { get; set; } = false;
    public bool AttemptRecoveryFromDisconnects { get; set; } = false;
    public bool ShowStatusInStatusBar { get; set; } = false;
    public bool ShowStatusAsIcons { get; set; } = false;
    public bool TrackDisabledCharacters { get; set; } = false;

    // Character & Window storages
    public WindowOptions Window { get; set; } = new();
    public Dictionary<string, CharacterOptions> Characters { get; set; } = new();
    public Dictionary<string, int> CollectedTomes { get; set; } = new();

    // Duty Options
    public uint ZoneId { get; set; } = TomestoneZone.AvailableTomestoneZones[0];
    public bool StopRunnerGracefully { get; set; } = true;
    public bool UseBossModRebornAI { get; set; } = true;

    // Stop Actions
    public StopAction StopAction { get; set; } = StopAction.None;
    public string CharacterForSwap { get; set; } = "";

    // Runner Options (General)
    public bool Repair { get; set; } = false;
    public bool RepairSelf { get; set; } = false;
    public uint RepairPercentage { get; set; } = 80;
    public bool Extract { get; set; } = false;
    public bool ExtractAll { get; set; } = false;
    public bool SpendUncappedTomestones { get; set; } = false;
    public uint SpendUncappedTomestoneThreshold { get; set; } = 1800;
    public string? SpendUncappedTomestoneItemName { get; set; } = null;

    // Runner Options (AutoRetainer)
    public bool AutoRetainerEnabled { get; set; } = false;
    public uint AutoRetainerThreshold { get; set; } = 90;
    // TODO: Add summoning bell location here...

    // Runner Options (Deliveroo)
    public bool DeliverooEnabled { get; set; } = false;
    public bool DeliverooRunOnFirstLoop { get; set; } = false;
    public bool DeliverooOnInterval { get; set; } = false;
    public uint DeliverooRunInterval { get; set; } = 2;

    public void Save()
    {
        AWC.PluginInterface.SavePluginConfig(this);
    }

    public int GetWeeklyTomes(string character)
    {
        return CollectedTomes.GetValueOrDefault(character, 0);
    }

    public List<string> GetSortedCharacters()
    {
        var characters = new List<string>(Characters.Keys);

        characters.Sort((a, b) =>
        {
            var aOption = GetOrRegisterCharacterOptions(a);
            var bOption = GetOrRegisterCharacterOptions(b);

            var compare = aOption.Position.CompareTo(bOption.Position);

            return compare != 0
                       ? compare
                       : string.Compare(a, b, StringComparison.Ordinal);
        });

        return characters;
    }

    public bool NormalizeCharacterPositions()
    {
        var sortedCharacters = GetSortedCharacters();
        var changed = false;

        for (var index = 0; index < sortedCharacters.Count; index++)
        {
            var character = sortedCharacters[index];
            var options = GetOrRegisterCharacterOptions(character);
            var newPosition = (uint)index;

            if (options.Position == newPosition)
                continue;

            options.Position = newPosition;
            changed = true;
        }

        return changed;
    }

    public CharacterOptions GetOrRegisterCharacterOptions(string character)
    {
        if (Characters.TryGetValue(character, out var value))
            return value;

        uint nextPosition = 0;
        foreach (var options in Characters.Values)
        {
            if (options.Position >= nextPosition)
                nextPosition = options.Position + 1;
        }

        return Characters[character] = new CharacterOptions { Position = nextPosition };
    }

    public bool IsRequiredSettingsSetup()
    {
        return TomestoneZone.IsSupportedTomestoneZone(ZoneId);
    }
}
