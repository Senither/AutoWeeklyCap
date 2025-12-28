using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace AutoWeeklyCap.Helpers;

public static class Character
{
    public static bool SwitchJob(uint targetJobId)
    {
        if (!AutoWeeklyCap.PlayerState.IsLoaded)
            return false;

        var currentJobId = AutoWeeklyCap.PlayerState.ClassJob.RowId;
        if (currentJobId == targetJobId)
            return false;

        unsafe
        {
            var gearsetModule = RaptureGearsetModule.Instance();
            if (gearsetModule == null)
                return false;

            for (byte i = 0; i < 100; i++)
            {
                if (!gearsetModule->IsValidGearset(i) || gearsetModule->GetGearset(i)->ClassJob != targetJobId)
                    continue;

                Chat.RunCommand($"gs change {i + 1}");
                return true;
            }
        }

        return false;
    }
}
