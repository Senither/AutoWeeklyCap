using System;
using Dalamud.Plugin.Services;

namespace AutoWeeklyCap;

public class FrameworkListener
{
    protected long enforceUpdateStateAt = 0;

    public void OnFrameworkUpdate(IFramework _)
    {
        var unixNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (enforceUpdateStateAt > unixNow)
            return;

        enforceUpdateStateAt = unixNow + 2;

        Utils.UpdateWeeklyAcquiredTomestonesForCurrentCharacter();

        Plugin.Runner.Tick();
    }
}
