using AutoWeeklyCap.Runner;

namespace AutoWeeklyCap.Config;

[Serializable]
public class CharacterOptions
{
    public bool Enabled { get; set; } = true;
    public bool Hidden { get; set; } = false;
    public PlayerJob PreferredJob { get; set; } = PlayerJob.None;
    public string? PreferredTomestoneItemName { get; set; } = null;
    public uint Position { get; set; } = 0;

    /// <summary>
    /// Checks if the character is both enabled and not hidden
    /// </summary>
    /// <returns></returns>
    public bool IsEnabled()
    {
        return Enabled && !Hidden;
    }

    public bool IsHidden()
    {
        return Hidden;
    }
}
