// ReSharper disable ArrangeMethodOrOperatorBody

namespace AutoWeeklyCap.Config;

[Serializable]
public class CharacterMetrics
{
    public uint RunsCompleted { get; set; } = 0;
    public ulong TimeSpentInRuns { get; set; } = 0;

    public uint UncappedAcquiredTomestoneCollected { get; set; } = 0;
    public uint WeeklyAcquiredLimitedTomestoneSpent { get; set; } = 0;
    public uint WeeklyAcquiredLimitedTomestoneCollected { get; set; } = 0;

    public uint RepairsCompleted { get; set; } = 0;
    public uint GilSpentOnRepairs { get; set; } = 0;
    public uint DarkMatterSpentOnRepairs { get; set; } = 0;

    public uint MateriaExtracted { get; set; } = 0;

    public uint RetainersCollected { get; set; } = 0;

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

    public void IncrementMateriaCounter() => MateriaExtracted++;
}
