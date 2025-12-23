using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap;

public class Utils
{
    public static string? GetFullCharacterName()
    {
        if (!Plugin.PlayerState.IsLoaded)
            return null;

        var world = Plugin.PlayerState.HomeWorld.ValueNullable;
        if (world == null)
            return null;
        
        return Plugin.PlayerState.CharacterName + "@" + world.Value.Name.ToString();
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
        catch (Exception e)
        {
            return 0;
        }
    }

    public static bool UpdateWeeklyAcquiredTomestonesForCurrentCharacter(Configuration configuration)
    {
        var character = GetFullCharacterName();
        if (character == null)
            return false;

        var tomes = GetWeeklyAcquiredTomestoneCount();

        if (configuration.CollectedTomes.GetValueOrDefault(character) == tomes)
            return false;
        
        configuration.CollectedTomes[character] = tomes;
        configuration.Save();

        return true;
    }
}
