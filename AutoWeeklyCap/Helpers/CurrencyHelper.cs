using AutoWeeklyCap.Config;

using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Helpers;

public static class CurrencyHelper
{
    public static int GetGil()
    {
        try {
            unsafe {
                return InventoryManager.Instance()->GetInventoryItemCount(1);
            }
        } catch (Exception) {
            return 0;
        }
    }

    public static int GetMGP()
    {
        try {
            unsafe {
                return InventoryManager.Instance()->GetInventoryItemCount(29u);
            }
        } catch (Exception) {
            return 0;
        }
    }

    public static int GetUncappedAcquiredTomestoneCount()
    {
        try {
            unsafe {
                return InventoryManager.Instance()->GetInventoryItemCount(48);
            }
        } catch (Exception) {
            return 0;
        }
    }

    public static bool IsPlayerLimitedTomestoneCapped()
    {
        return IsPlayerWeeklyLimitedTomestoneCapped() || IsPlayerTotalLimitedTomestoneCapped();
    }

    public static bool IsPlayerWeeklyLimitedTomestoneCapped()
    {
        return GetLimitedTomestoneWeeklyLimit() == GetWeeklyAcquiredLimitedTomestoneCount();
    }

    public static bool IsPlayerTotalLimitedTomestoneCapped()
    {
        return GetTotalAcquiredLimitedTomestoneCount() == Constants.LimitedCurrencyCap;
    }

    public static int GetLimitedTomestoneWeeklyLimit()
    {
        return InventoryManager.GetLimitedTomestoneWeeklyLimit();
    }

    public static int GetWeeklyAcquiredLimitedTomestoneCount()
    {
        try {
            unsafe {
                return InventoryManager.Instance()->GetWeeklyAcquiredTomestoneCount();
            }
        } catch (Exception) {
            return 0;
        }
    }

    public static uint GetTotalAcquiredLimitedTomestoneCount()
    {
        try {
            unsafe {
                return InventoryManager.Instance()->GetTomestoneCount(49u);
            }
        } catch (Exception) {
            return 0;
        }
    }

    public static bool UpdateWeeklyAcquiredTomestonesForCurrentCharacter()
    {
        var characterAndWorld = PlayerHelper.GetFullCharacterName();
        if (characterAndWorld == null) {
            return false;
        }

        var options = AWC.Config.GetOrRegisterCharacterOptions(Player.CID, characterAndWorld);
        if (!options.IsEnabled() && !AWC.Config.TrackDisabledCharacters) {
            return false;
        }

        var limitedTomes = GetTotalAcquiredLimitedTomestoneCount();
        var limitedTomesChanged = limitedTomes != options.TotalAcquiredLimitedTomestones;
        if (limitedTomesChanged) {
            options.TotalAcquiredLimitedTomestones = limitedTomes;
        }

        var weeklyTomes = GetWeeklyAcquiredLimitedTomestoneCount();
        var storedTomes = AWC.Config.CollectedTomes.GetValueOrDefault(characterAndWorld);

        var weeklyTomesReset = storedTomes > weeklyTomes;
        if (weeklyTomesReset) {
            AWC.Config.CollectedTomes.Clear();
        }

        if (!limitedTomesChanged && !weeklyTomesReset && weeklyTomes == storedTomes) {
            return false;
        }

        AWC.Config.CollectedTomes[characterAndWorld] = weeklyTomes;
        Configuration.Save();

        return true;
    }
}
