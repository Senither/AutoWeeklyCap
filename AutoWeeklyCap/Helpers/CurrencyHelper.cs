using System;
using System.Collections.Generic;
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

        var options = AutoWeeklyCap.Config.GetOrRegisterCharacterOptions(characterAndWorld);
        if (!options.IsEnabled() && !AutoWeeklyCap.Config.TrackDisabledCharacters)
            return false;

        var weeklyTomes = GetWeeklyAcquiredTomestoneCount();
        var storedTomes = AutoWeeklyCap.Config.CollectedTomes.GetValueOrDefault(characterAndWorld);

        if (weeklyTomes == storedTomes)
            return false;

        if (storedTomes > weeklyTomes)
        {
            AutoWeeklyCap.Config.CollectedTomes.Clear();
        }

        AutoWeeklyCap.Config.CollectedTomes[characterAndWorld] = weeklyTomes;
        AutoWeeklyCap.Config.Save();

        return true;
    }
}
