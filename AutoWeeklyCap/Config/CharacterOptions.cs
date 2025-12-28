using System;

namespace AutoWeeklyCap.Config;

[Serializable]
public class CharacterOptions
{
    public bool Enabled { get; set; } = true;
    public bool Hidden { get; set; } = false;

    /**
     * Checks if the character is both enabled and not hidden.
     */
    public bool IsEnabled()
    {
        return Enabled && !Hidden;
    }

    public bool IsHidden()
    {
        return Hidden;
    }
}
