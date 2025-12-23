using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace AutoWeeklyCap;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public uint ZoneId { get; set; } = 0;
    public string[] Characters { get; set; } = ["", "", "", "", "", "", "", "", ""];
    
    public Dictionary<string, int> CollectedTomes { get; set; } = new();

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public int GetWeeklyTomes(string character)
    {
        return CollectedTomes.GetValueOrDefault(character, 0);
    }
}
