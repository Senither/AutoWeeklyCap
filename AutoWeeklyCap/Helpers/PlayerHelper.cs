using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace AutoWeeklyCap.Helpers;

public enum CharacterSwapStatus
{
    FailedToSwitchJob = 0,
    SwitchedJob = 1,
    AlreadyOnTargetJob = 2
}

public static class PlayerHelper
{
    public static CharacterSwapStatus SwitchJob(uint targetJobId)
    {
        if (!AutoWeeklyCap.PlayerState.IsLoaded)
            return CharacterSwapStatus.FailedToSwitchJob;

        var currentJobId = AutoWeeklyCap.PlayerState.ClassJob.RowId;
        if (currentJobId == targetJobId)
            return CharacterSwapStatus.AlreadyOnTargetJob;

        unsafe
        {
            var gearsetModule = RaptureGearsetModule.Instance();
            if (gearsetModule == null)
                return CharacterSwapStatus.FailedToSwitchJob;

            for (byte i = 0; i < 100; i++)
            {
                if (!gearsetModule->IsValidGearset(i) || gearsetModule->GetGearset(i)->ClassJob != targetJobId)
                    continue;

                Chat.RunCommand($"gs change {i + 1}");
                return CharacterSwapStatus.SwitchedJob;
            }
        }

        return CharacterSwapStatus.FailedToSwitchJob;
    }
    
    internal static unsafe uint GetGrandCompanyTerritoryType(GrandCompany grandCompany) => grandCompany switch
    {
        GrandCompany.Maelstrom => 128u,
        GrandCompany.TwinAdder => 132u,
        _ => 130u
    };

    internal static unsafe GrandCompany GetGrandCompany() => (GrandCompany) PlayerState.Instance()->GrandCompany;
}
