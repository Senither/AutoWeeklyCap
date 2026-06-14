using AutoWeeklyCap.Contracts.Runner;

namespace AutoWeeklyCap.Runner.Actions.Safezones;

public class EnterApartmentNamedTasks : BaseNamedTasks
{
    protected override string Name => nameof(EnterApartmentNamedTasks);
    protected override string[] AddonsToClose { get; } = ["SelectYesno", "SelectString", "HousingMenu"];

    private const int LongTaskTimeout = 120_000;

    protected override bool Run(params object[] args)
    {
        if (!LifestreamIPC.IsEnabled) {
            return false;
        }

        if (HousingHelper.IsInsideApartment()) {
            return false;
        }

        if (!LifestreamIPC.HasApartment()) {
            LogDebug("Player does not have an apartment");
            return false;
        }

        using var title = TitleManager.RegisterTitle(BitmapFontIcon.WatchingCutscene, "Entering apartment");

        Enqueue(() =>
        {
            if (!EzThrottler.Throttle("NavigatingToApartmentPlot", 500)) {
                return false;
            }

            if (LifestreamIPC.IsBusy()) {
                return false;
            }

            LifestreamIPC.EnterApartment(true);
            return true;
        }, "teleport to apartment");

        Enqueue(
            () => HousingHelper.IsInsideApartment() && PlayerHelper.IsReady && !LifestreamIPC.IsBusy(),
            "wait for apartment teleport",
            LongTaskTimeout
        );

        return true;
    }
}
