using AutoWeeklyCap.Helpers;

// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.Runner;

public enum PlayerJob
{
    None = 0,

    // Tanks
    PLD = 19,
    WAR = 21,
    DRK = 32,
    GNB = 37,

    // Healers   
    WHM = 24,
    SCH = 28,
    AST = 33,
    SGE = 40,

    // Melees
    MNK = 20,
    DRG = 22,
    NIN = 30,
    SAM = 34,
    RPR = 39,
    VPR = 41,

    // Physical Ranged
    BRD = 23,
    MCH = 31,
    DNC = 38,

    // Casters
    BLM = 25,
    SMN = 27,
    RDM = 35,
    PCT = 42
}

public static class PlayerJobExtensions
{
    public static string GetName(this PlayerJob job)
    {
        return job.ToString();
    }

    public static void SwitchToJob(this PlayerJob job)
    {
        switch (job)
        {
            case PlayerJob.None:
                break;

            default:
                Character.SwitchJob((uint)job);
                break;
        }
    }

    public static PlayerJob[] GetValues()
    {
        return
        [
            // Default
            PlayerJob.None,

            // Tanks
            PlayerJob.PLD, PlayerJob.WAR, PlayerJob.DRK, PlayerJob.GNB,
            // Healers   
            PlayerJob.WHM, PlayerJob.SCH, PlayerJob.AST, PlayerJob.SGE,
            // Melees
            PlayerJob.MNK, PlayerJob.DRG, PlayerJob.NIN, PlayerJob.SAM, PlayerJob.RPR, PlayerJob.VPR,
            // Physical Ranged
            PlayerJob.BRD, PlayerJob.MCH, PlayerJob.DNC,
            // Casters
            PlayerJob.BLM, PlayerJob.SMN, PlayerJob.RDM, PlayerJob.PCT
        ];
    }
}
