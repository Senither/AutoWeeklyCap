using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace AutoWeeklyCap.Config;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    
    public WindowOptions Window { get; set; } = new();
    
    public uint ZoneId { get; set; } = 0;
    public Dictionary<string, CharacterOptions> Characters { get; set; } = new(); 

    public Dictionary<string, int> CollectedTomes { get; set; } = new();
    
    public bool StopRunnerGracefully { get; set; } = false;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public int GetWeeklyTomes(string character)
    {
        return CollectedTomes.GetValueOrDefault(character, 0);
    }

    public CharacterOptions GetOrRegisterCharacterOptions(string character)
    {
        if (Characters.TryGetValue(character, out var value))
            return value;

        return Characters[character] = new CharacterOptions();
    }
}
