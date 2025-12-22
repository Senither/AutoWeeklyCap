using Dalamud.Configuration;
using System;

namespace AutoWeeklyCap;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public uint ZoneId { get; set; } = 0;
    public string[] Characters { get; set; } = ["", "", "", "", "", "", "", "", ""];

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public int GetWeeklyTomes(string character)
    {
        return 0;
    }
}
