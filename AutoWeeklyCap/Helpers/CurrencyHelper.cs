using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Helpers;

public static class CurrencyHelper
{
    public static int GetUncappedAcquiredTomestoneCount()
    {
        try
        {
            unsafe
            {
                return InventoryManager.Instance()->GetInventoryItemCount(48);
            }
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public static bool IsPlayerLimitedTomestoneCapped()
    {
        return GetLimitedTomestoneWeeklyLimit() == GetWeeklyAcquiredTomestoneCount();
    }

    public static int GetLimitedTomestoneWeeklyLimit()
    {
        return InventoryManager.GetLimitedTomestoneWeeklyLimit();
    }

    public static int GetWeeklyAcquiredTomestoneCount()
    {
        try
        {
            unsafe
            {
                return InventoryManager.Instance()->GetWeeklyAcquiredTomestoneCount();
            }
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public static bool UpdateWeeklyAcquiredTomestonesForCurrentCharacter()
    {
        var characterAndWorld = PlayerHelper.GetFullCharacterName();
        if (characterAndWorld == null)
            return false;

        var options = AWC.Config.GetOrRegisterCharacterOptions(characterAndWorld);
        if (!options.IsEnabled() && !AWC.Config.TrackDisabledCharacters)
            return false;

        var weeklyTomes = GetWeeklyAcquiredTomestoneCount();
        var storedTomes = AWC.Config.CollectedTomes.GetValueOrDefault(characterAndWorld);

        if (weeklyTomes == storedTomes)
            return false;

        if (storedTomes > weeklyTomes)
        {
            AWC.Config.CollectedTomes.Clear();
        }

        AWC.Config.CollectedTomes[characterAndWorld] = weeklyTomes;
        AWC.Config.Save();

        return true;
    }
}
