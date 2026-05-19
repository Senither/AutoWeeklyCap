namespace AutoWeeklyCap.Config;

[Serializable]
public class LevelJobsOptions
{
    public bool UseCharacterOrder { get; set; } = true;
    public bool UseStylistForGearUpgrades { get; set; } = true;
    public string SelectedCharacter { get; set; } = string.Empty;
    public Dictionary<string, List<LevelJobEntry>> CharacterJobs { get; set; } = new();
}

[Serializable]
public class LevelJobEntry
{
    public PlayerJob Job { get; set; } = PlayerJob.None;
    public bool Enabled { get; set; } = true;
}
