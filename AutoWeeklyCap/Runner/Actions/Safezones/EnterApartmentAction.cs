using AutoWeeklyCap.Contracts.Runner;

namespace AutoWeeklyCap.Runner.Actions.Safezones;

public class EnterApartmentAction : BaseAction
{
    protected override string Name => nameof(EnterApartmentAction);
    protected override string[] AddonsToClose { get; } = ["SelectYesno", "SelectString", "HousingMenu"];

    private const int LongTaskTimeout = 120_000;
    private const string LifestreamApartmentCommand = "Apartment";

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

        Enqueue(
            () => MovementHelper.TeleportTo(
                LifestreamApartmentCommand,
                () => HousingHelper.IsInsideApartment() && PlayerHelper.IsReady && !LifestreamIPC.IsBusy()
            ),
            "teleport to apartment"
        );

        return true;
    }
}
