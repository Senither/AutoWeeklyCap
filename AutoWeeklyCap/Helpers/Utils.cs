using System;
using System.Collections.Generic;
using AutoWeeklyCap.IPC;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Helpers;

public static class Utils
{
    public static string? GetZoneNameFromId(uint zoneId)
    {
        if (AutoWeeklyCap.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(zoneId, out var territoryRow))
        {
            var name = territoryRow.PlaceName.Value.Name.ExtractText();

            return name.Length == 0 ? null : name;
        }

        return null;
    }

    public static string? GetFullCharacterName()
    {
        if (!AutoWeeklyCap.PlayerState.IsLoaded)
            return null;

        var world = AutoWeeklyCap.PlayerState.HomeWorld.ValueNullable;
        if (world == null)
            return null;

        return AutoWeeklyCap.PlayerState.CharacterName + "@" + world.Value.Name.ToString();
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
        catch (Exception _)
        {
            return 0;
        }
    }

    public static bool UpdateWeeklyAcquiredTomestonesForCurrentCharacter()
    {
        var character = GetFullCharacterName();
        if (character == null)
            return false;

        var options = AutoWeeklyCap.Config.GetOrRegisterCharacterOptions(character);
        if (!options.IsEnabled())
            return false;

        var tomes = GetWeeklyAcquiredTomestoneCount();
        if (AutoWeeklyCap.Config.CollectedTomes.GetValueOrDefault(character) == tomes)
            return false;

        AutoWeeklyCap.Config.CollectedTomes[character] = tomes;
        AutoWeeklyCap.Config.Save();

        return true;
    }

    public static bool IsRequiredPluginsEnabled()
    {
        return LifestreamIPC.IsEnabled && AutoDutyIPC.IsEnabled;
    }

    public static bool IsPluginEnabled(string name)
    {
        foreach (var plugin in AutoWeeklyCap.PluginInterface.InstalledPlugins)
        {
            if (plugin.InternalName == name && plugin.IsLoaded)
                return true;
        }

        return false;
    }
}
