using System;
using System.Collections.Generic;
using AutoWeeklyCap.IPC;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

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
        catch (Exception _)
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
