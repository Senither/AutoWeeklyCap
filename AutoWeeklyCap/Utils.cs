using System;
using System.Collections.Generic;
using AutoWeeklyCap.IPC;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Lumina.Excel.Sheets;

namespace AutoWeeklyCap;

public class Utils
{
    public static string? GetZoneNameFromId(uint zoneId)
    {
        if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(zoneId, out var territoryRow))
        {
            var name = territoryRow.PlaceName.Value.Name.ExtractText();
            
            return name.Length == 0 ? null : name;
        }

        return null;
    }

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

        var options = Plugin.Config.GetOrRegisterCharacterOptions(character);
        if (!options.Enabled)
            return false;

        var tomes = GetWeeklyAcquiredTomestoneCount();
        if (Plugin.Config.CollectedTomes.GetValueOrDefault(character) == tomes)
            return false;

        Plugin.Config.CollectedTomes[character] = tomes;
        Plugin.Config.Save();

        return true;
    }

    public static bool IsRequiredPluginsEnabled()
    {
        return LifestreamIPC.IsEnabled && AutoDutyIPC.IsEnabled;
    }

    public static bool IsPluginEnabled(string name)
    {
        foreach (var plugin in Plugin.PluginInterface.InstalledPlugins)
        {
            if (plugin.InternalName == name && plugin.IsLoaded)
                return true;
        }

        return false;
    }

    public static bool RunShellCommand(string commandString)
    {
        try
        {
            unsafe
            {
                var command = new Utf8String(
                    commandString.StartsWith('/')
                        ? commandString
                        : "/" + commandString
                );

                RaptureShellModule.Instance()->ExecuteCommandInner(&command, UIModule.Instance());
            }

            return true;
        }
        catch (Exception _)
        {
            return false;
        }
    }
}
