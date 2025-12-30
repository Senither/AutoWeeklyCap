using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace AutoWeeklyCap.Helpers;

public enum CharacterSwapStatus
{
    FailedToSwitchJob = 0,
    SwitchedJob = 1,
    AlreadyOnTargetJob = 2
}

public static class Character
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
}
