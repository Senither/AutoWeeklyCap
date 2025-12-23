using System;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap;

public class FrameworkListener
{
    protected Configuration Configuration;
    protected long enforceUpdateStateAt = 0;

    public FrameworkListener(Plugin plugin)
    {
        Configuration = plugin.Configuration;
    }

    public void OnFrameworkUpdate(IFramework _)
    {
        if (!Plugin.ClientState.IsLoggedIn)
            return;

        var unixNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (enforceUpdateStateAt > unixNow)
            return;

        // TODO: Check if the player is online and available, and then set some states that
        // can be checked before starting the auto duty or relogging of characters.

        enforceUpdateStateAt = unixNow + 15;

        updateTomesForCharacter();
    }

    private void updateTomesForCharacter()
    {
        if (!Plugin.PlayerState.IsLoaded)
            return;

        var character = Plugin.PlayerState.CharacterName + "@" + Plugin.PlayerState.HomeWorld.Value.Name.ToString();

        unsafe
        {
            var count = InventoryManager.Instance()->GetWeeklyAcquiredTomestoneCount();

            Configuration.CollectedTomes[character] = count;
            Configuration.Save();
        }
    }
}
