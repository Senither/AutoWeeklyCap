using Dalamud.Configuration;

// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.Config;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool DevMode { get; set; } = false;

    // General Options
    public bool StartRunnerOnBoot { get; set; } = false;
    public bool OpenWindowOnStartup { get; set; } = false;
    public bool UseSliders { get; set; } = false;
    public bool AttemptRecoveryFromDisconnects { get; set; } = true;
    public bool DisableTitleScreenMovie { get; set; } = false;
    public bool ShowStatusInStatusBar { get; set; } = false;
    public bool ShowStatusAsIcons { get; set; } = false;
    public bool TrackDisabledCharacters { get; set; } = true;

    // Character & Window storages
    public ColorTheme SelectedColorTheme { get; set; } = ColorTheme.Indigo;
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
    public bool AlwaysStartOnHomeWorld { get; set; } = true;
    public bool OnlyStartAutoDutyFromGCInn { get; set; } = false;
    public bool Repair { get; set; } = true;
    public bool RepairSelf { get; set; } = true;
    public uint RepairPercentage { get; set; } = 50;
    public bool Extract { get; set; } = true;
    public bool ExtractAll { get; set; } = false;
    public bool SpendUncappedTomestones { get; set; } = false;
    public uint SpendUncappedTomestoneThreshold { get; set; } = 1800;
    public string? SpendUncappedTomestoneItemName { get; set; } = null;

    // Runner Options (AutoRetainer)
    public bool AutoRetainerEnabled { get; set; } = false;
    public uint AutoRetainerThreshold { get; set; } = 90;
    public RetainerTrigger AutoRetainerTrigger { get; set; } = RetainerTrigger.AnyCharacter;

    // Runner Options (Deliveroo)
    public bool DeliverooEnabled { get; set; } = false;
    public bool DeliverooRunOnFirstLoop { get; set; } = false;
    public bool DeliverooOnInterval { get; set; } = true;
    public uint DeliverooRunInterval { get; set; } = 2;

    // Runner Options (Notification Master)
    public bool NotificationMasterEnabled { get; set; } = false;
    public bool NotificationMasterUsingOnRunnerStopped { get; set; } = true;
    public bool NotificationMasterUsingOnFullyCapped { get; set; } = true;
    public bool NotificationMasterUsingFlashTaskbarIcon { get; set; } = true;
    public bool NotificationMasterUsingToastNotification { get; set; } = true;
    public bool NotificationMasterUsingPlaySound { get; set; } = false;
    public bool NotificationMasterUsingPlaySoundOptionRepeat { get; set; } = false;
    public bool NotificationMasterUsingPlaySoundOptionStopOnFocus { get; set; } = false;
    public uint NotificationMasterUsingPlaySoundOptionVolume { get; set; } = 50;
    public string NotificationMasterUsingPlaySoundOptionFilePath { get; set; } = "";

    public void Save()
    {
        AWC.PluginInterface.SavePluginConfig(this);
    }

    public int GetWeeklyTomes(string character)
    {
        return CollectedTomes.GetValueOrDefault(character, 0);
    }

    public string? GetFirstUncappedCharacter()
    {
        var limit = CurrencyHelper.GetLimitedTomestoneWeeklyLimit();

        foreach (var character in GetSortedCharacters()) {
            var option = GetOrRegisterCharacterOptions(character);
            if (option == null || !option.IsEnabled()) {
                continue;
            }

            var tomes = CollectedTomes.GetValueOrDefault(character, 0);
            if (tomes == limit) {
                continue;
            }

            return character;
        }

        return null;
    }

    public List<string> GetSortedCharacters()
    {
        var characters = new List<string>(Characters.Keys);

        characters.Sort((a, b) =>
        {
            var aOption = GetOrRegisterCharacterOptions(a)!;
            var bOption = GetOrRegisterCharacterOptions(b)!;

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

        for (var index = 0; index < sortedCharacters.Count; index++) {
            var character = sortedCharacters[index];
            var options = GetOrRegisterCharacterOptions(character);
            if (options == null) {
                continue;
            }

            var newPosition = (uint)index;
            if (options.Position == newPosition) {
                continue;
            }

            options.Position = newPosition;
            changed = true;
        }

        return changed;
    }

    public CharacterOptions? GetOrRegisterCharacterOptions(string character)
    {
        return Characters.GetValueOrDefault(character);
    }

    public CharacterOptions GetOrRegisterCharacterOptions(ulong id, string character)
    {
        var option = Characters.GetValueOrDefault(character);
        if (option != null) {
            return ApplyCharacterPropertiesToOptions(option, id, character);
        }

        var keyPair = Characters.FirstOrDefault(item => item.Value.ID == id);
        if (keyPair.Value != null) {
            AWC.Log.Info($"Config: Found renamed character with ID {id}, character: {keyPair.Value.Name}@{keyPair.Value.World} => {character}");

            Characters.Remove(keyPair.Key);
            Characters[character] = ApplyCharacterPropertiesToOptions(keyPair.Value, id, character);
            Save();

            return Characters[character];
        }

        uint nextPosition = 0;
        foreach (var options in Characters.Values) {
            if (options.Position >= nextPosition) {
                nextPosition = options.Position + 1;
            }
        }

        AWC.Log.Info($"Config: Registering new character: {character} (id: {id})");
        Characters[character] = ApplyCharacterPropertiesToOptions(new CharacterOptions { Position = nextPosition }, id, character);
        Save();

        return Characters[character];
    }

    public void SetColorTheme(ColorTheme theme)
    {
        SelectedColorTheme = theme;
        Theme.Primary = theme.GetPrimaryColor();
    }

    private CharacterOptions ApplyCharacterPropertiesToOptions(CharacterOptions option, ulong id, string character)
    {
        var wasChanged = false;

        if (option.ID != id) {
            wasChanged = true;
            option.ID = id;
        }

        var parts = character.Split("@");
        if (parts.Length == 2 && (parts[0] != option.Name || parts[1] != option.World)) {
            wasChanged = true;
            option.Name = parts[0];
            option.World = parts[1];
        }

        if (wasChanged) {
            Save();
        }

        return option;
    }

    public bool IsRequiredSettingsSetup()
    {
        return TomestoneZone.IsSupportedTomestoneZone(ZoneId);
    }
}
