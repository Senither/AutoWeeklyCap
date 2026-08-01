namespace AutoWeeklyCap.Config;

[Serializable]
public class CharacterMetrics
{
    public uint RunsCompleted { get; private set; } = 0;
    public ulong TimeSpentInRuns { get; private set; } = 0;

    public uint UncappedAcquiredTomestoneCollected { get; private set; } = 0;
    public uint WeeklyAcquiredLimitedTomestoneSpent { get; private set; } = 0;
    public uint WeeklyAcquiredLimitedTomestoneCollected { get; private set; } = 0;

    public uint RepairsCompleted { get; private set; } = 0;
    public uint GilSpentOnRepairs { get; private set; } = 0;
    public uint DarkMatterSpentOnRepairs { get; private set; } = 0;

    public uint MateriaExtracted { get; private set; } = 0;

    public uint RetainersCollected { get; private set; } = 0;

    public void IncrementRunsCounter(int durationInSeconds)
    {
        RunsCompleted++;
        TimeSpentInRuns += (ulong)durationInSeconds;
    }

    public void IncrementRepairsCounter(uint gilSpent = 0, uint darkMatterSpent = 0)
    {
        RepairsCompleted++;
        GilSpentOnRepairs += gilSpent;
        DarkMatterSpentOnRepairs += darkMatterSpent;
    }
}
