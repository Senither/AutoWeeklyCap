using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.Helpers;

public static class CurrencyHelper
{
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

        var weeklyTomesChanged = storedTomes > weeklyTomes;
        if (weeklyTomesChanged) {
            AWC.Config.CollectedTomes.Clear();
        }

        if (!limitedTomesChanged && !weeklyTomesChanged) {
            return false;
        }

        AWC.Config.CollectedTomes[characterAndWorld] = weeklyTomes;
        AWC.Config.Save();

        return true;
    }
}
