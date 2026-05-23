namespace AutoWeeklyCap.Config;

[Serializable]
public class CharacterOptions
{
    // ReSharper disable once InconsistentNaming
    public ulong ID { get; set; } = 0u;
    public string Name { get; set; } = string.Empty;
    public string World { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool Hidden { get; set; } = false;
    public PlayerJob PreferredJob { get; set; } = PlayerJob.None;
    public string? PreferredTomestoneItemName { get; set; } = null;
    public Safezone? PreferredSafezone { get; set; } = null;
    public uint TotalAcquiredLimitedTomestones { get; set; } = 0;
    public uint Position { get; set; } = 0;
    public List<int> LastDutyDurationsSeconds { get; set; } = [];
    public Dictionary<PlayerJob, int> JobLevels { get; set; } = new();

    private const int MaxDutySamples = 5;

    /// <summary>
    /// Checks if the character is both enabled and not hidden
    /// </summary>
    /// <returns></returns>
    public bool IsEnabled()
    {
        return Enabled && !Hidden;
    }

    public bool IsHidden()
    {
        return Hidden;
    }

    public bool IsTotalAcquiredLimitedTomestoneCapped()
    {
        return TotalAcquiredLimitedTomestones == Constants.LimitedCurrencyCap;
    }

    public bool HasOverrideSettingsEnabled()
    {
        return PreferredTomestoneItemName != null || PreferredSafezone != null;
    }

    public void AddDutyDurationSeconds(int durationSeconds)
    {
        if (durationSeconds <= 0) {
            return;
        }

        LastDutyDurationsSeconds.Add(durationSeconds);

        while (LastDutyDurationsSeconds.Count > MaxDutySamples) {
            LastDutyDurationsSeconds.RemoveAt(0);
        }
    }
}
