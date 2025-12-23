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
        var unixNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (enforceUpdateStateAt > unixNow)
            return;

        enforceUpdateStateAt = unixNow + 5;

        Utils.UpdateWeeklyAcquiredTomestonesForCurrentCharacter(Configuration);

        Plugin.Runner.Tick();
    }
}
