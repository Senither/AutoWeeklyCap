// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.Enums;

public enum PlayerJob
{
    None = 0,

    // Crafters
    CPR = 8,
    BSM = 9,
    ARM = 10,
    GSM = 11,
    LTW = 12,
    WVR = 13,
    ALC = 14,
    CUL = 15,

    // Gathers
    MIN = 16,
    BTN = 17,
    FSH = 18,

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
    extension(PlayerJob job)
    {
        public string GetName()
        {
            return job.ToString();
        }

        public BitmapFontIcon GetIcon()
        {
            return job switch
            {
                PlayerJob.None => BitmapFontIcon.AnyClass,
                PlayerJob.CPR => BitmapFontIcon.Carpenter,
                PlayerJob.BSM => BitmapFontIcon.Blacksmith,
                PlayerJob.ARM => BitmapFontIcon.Armorer,
                PlayerJob.GSM => BitmapFontIcon.Goldsmith,
                PlayerJob.LTW => BitmapFontIcon.Leatherworker,
                PlayerJob.WVR => BitmapFontIcon.Weaver,
                PlayerJob.ALC => BitmapFontIcon.Alchemist,
                PlayerJob.CUL => BitmapFontIcon.Culinarian,
                PlayerJob.MIN => BitmapFontIcon.Miner,
                PlayerJob.BTN => BitmapFontIcon.Botanist,
                PlayerJob.FSH => BitmapFontIcon.Fisher,
                PlayerJob.PLD => BitmapFontIcon.Paladin,
                PlayerJob.WAR => BitmapFontIcon.Warrior,
                PlayerJob.DRK => BitmapFontIcon.DarkKnight,
                PlayerJob.GNB => BitmapFontIcon.Gunbreaker,
                PlayerJob.WHM => BitmapFontIcon.WhiteMage,
                PlayerJob.SCH => BitmapFontIcon.Scholar,
                PlayerJob.AST => BitmapFontIcon.Astrologian,
                PlayerJob.SGE => BitmapFontIcon.Sage,
                PlayerJob.MNK => BitmapFontIcon.Monk,
                PlayerJob.DRG => BitmapFontIcon.Dragoon,
                PlayerJob.NIN => BitmapFontIcon.Ninja,
                PlayerJob.SAM => BitmapFontIcon.Samurai,
                PlayerJob.RPR => BitmapFontIcon.Reaper,
                PlayerJob.VPR => BitmapFontIcon.Viper,
                PlayerJob.BRD => BitmapFontIcon.Bard,
                PlayerJob.MCH => BitmapFontIcon.Machinist,
                PlayerJob.DNC => BitmapFontIcon.Dancer,
                PlayerJob.BLM => BitmapFontIcon.BlackMage,
                PlayerJob.SMN => BitmapFontIcon.Summoner,
                PlayerJob.RDM => BitmapFontIcon.RedMage,
                PlayerJob.PCT => BitmapFontIcon.Pictomancer,
                _ => BitmapFontIcon.AnyClass
            };
        }

        public bool SwitchToJob()
        {
            if (!EzThrottler.Throttle("SwitchToPreferredJob", 250)) {
                return false;
            }

            if (job == PlayerJob.None) {
                return true;
            }

            var status = PlayerHelper.SwitchJob((uint)job);

            AWC.Log.Debug($"PlayerJob: Attempted to switch to job {job}, got status: {status}");
            return status == CharacterSwapStatus.AlreadyOnTargetJob;
        }

        public bool IsAlreadyOnJob()
        {
            if (job == PlayerJob.None) {
                return true;
            }

            return AWC.PlayerState.ClassJob.RowId == (uint)job;
        }
    }

    public static PlayerJob[] GetSelectableCombatJobs()
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
