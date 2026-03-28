using ECommons.Logging;

namespace AutoWeeklyCap.Commands;

public class DevModeBaseCommand : BaseCommand
{
    public override string[] Triggers { get; } = ["devmode"];
    public override string Description => "Toggles developer mode on and off";

    public DevModeBaseCommand()
    {
        Hidden = true;
    }

    public override void Run(string[] args)
    {
        AWC.Config.DevMode = !AWC.Config.DevMode;

        AWC.Instance.OpenConfigUi(
            AWC.Config.DevMode
                ? SettingsWindowOption.DeveloperToolbox
                : SettingsWindowOption.GeneralOptions
        );

        DuoLog.Information($"Developer mode has been {(AWC.Config.DevMode ? "enabled" : "disabled")}");
    }
}
