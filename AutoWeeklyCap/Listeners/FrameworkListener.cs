using System;
using AutoWeeklyCap.Helpers;
using Dalamud.Plugin.Services;

namespace AutoWeeklyCap.Listeners;

public class FrameworkListener
{
    protected long EnforceUpdateStateAt = 0;

    public void OnFrameworkUpdate(IFramework _)
    {
        var unixNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (EnforceUpdateStateAt > unixNow)
            return;

        EnforceUpdateStateAt = unixNow + 500;

        CurrencyHelper.UpdateWeeklyAcquiredTomestonesForCurrentCharacter();
        AutoWeeklyCap.Runner.Tick();
    }
}
