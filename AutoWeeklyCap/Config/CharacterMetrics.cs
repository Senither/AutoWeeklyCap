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

    public uint GilSpentOnTeleportationFees { get; set; } = 0;

    public uint DeliverableItemsHandedIn { get; set; } = 0;

    public void IncrementRunsCounter(int durationInSeconds, uint uncappedTomesCollected = 0, uint limitedTomesCollected = 0)
    {
        RunsCompleted++;
        TimeSpentInRuns += (ulong)durationInSeconds;

        UncappedAcquiredTomestoneCollected += uncappedTomesCollected;
        WeeklyAcquiredLimitedTomestoneCollected += limitedTomesCollected;
    }

    public void IncrementRepairsCounter(uint gilSpent = 0, uint darkMatterSpent = 0)
    {
        RepairsCompleted++;
        GilSpentOnRepairs += gilSpent;
        DarkMatterSpentOnRepairs += darkMatterSpent;
    }

    public void IncrementMateriaCounter() => MateriaExtracted++;
    public void IncrementWeeklyTomestoneSpentCounter(uint tomestones) => WeeklyAcquiredLimitedTomestoneSpent += tomestones;
    public void IncrementGilSpentOnTeleportationFeesCounter(uint gilSpent) => GilSpentOnTeleportationFees += gilSpent;
    public void IncrementDeliverableItemsHandedInCounter(uint deliverableItems) => DeliverableItemsHandedIn += deliverableItems;
}
