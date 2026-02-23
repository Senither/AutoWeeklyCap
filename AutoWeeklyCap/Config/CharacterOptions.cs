namespace AutoWeeklyCap.Config;

[Serializable]
public class CharacterOptions
{
    public bool Enabled { get; set; } = true;
    public bool Hidden { get; set; } = false;
    public PlayerJob PreferredJob { get; set; } = PlayerJob.None;
    public string? PreferredTomestoneItemName { get; set; } = null;
    public uint Position { get; set; } = 0;
    public List<int> LastDutyDurationsSeconds { get; set; } = [];

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

    public bool HasOverrideSettingsEnabled()
    {
        return PreferredTomestoneItemName != null;
    }

    public void AddDutyDurationSeconds(int durationSeconds)
    {
        if (durationSeconds <= 0)
            return;

        LastDutyDurationsSeconds.Add(durationSeconds);

        while (LastDutyDurationsSeconds.Count > MaxDutySamples)
            LastDutyDurationsSeconds.RemoveAt(0);
    }
}
