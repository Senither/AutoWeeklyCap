using AutoWeeklyCap.UI.ControlPanelWindow;

using Dalamud.Interface;

namespace AutoWeeklyCap.Enums;

public enum SettingsWindowOption
{
    GeneralOptions = 0,
    DutyOptions = 1,
    Characters = 2,
    RunnerOptions = 3,
    StopOptions = 4,
    ManuallyResetTomestone = 5,
    Changelog = 6,
    PluginInformationAndDependencies = 7,

    DeveloperToolbox = 99,
}

public static class SettingsWindowOptionsExtensions
{
    extension(SettingsWindowOption option)
    {
        public string GetName()
        {
            return option switch
            {
                SettingsWindowOption.GeneralOptions => "General Options",
                SettingsWindowOption.DutyOptions => "Duty Options",
                SettingsWindowOption.Characters => "Characters",
                SettingsWindowOption.RunnerOptions => "Runner Options",
                SettingsWindowOption.StopOptions => "Stop Actions",
                SettingsWindowOption.ManuallyResetTomestone => "Manually Reset Tomestones",
                SettingsWindowOption.Changelog => "Changelog",
                SettingsWindowOption.PluginInformationAndDependencies => "About & Dependencies",
                SettingsWindowOption.DeveloperToolbox => "Developer Toolbox",
                _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
            };
        }

        public FontAwesomeIcon GetIcon()
        {
            return option switch
            {
                SettingsWindowOption.GeneralOptions => FontAwesomeIcon.Computer,
                SettingsWindowOption.DutyOptions => FontAwesomeIcon.Gamepad,
                SettingsWindowOption.Characters => FontAwesomeIcon.Users,
                SettingsWindowOption.RunnerOptions => FontAwesomeIcon.Gamepad,
                SettingsWindowOption.StopOptions => FontAwesomeIcon.StopCircle,
                SettingsWindowOption.ManuallyResetTomestone => FontAwesomeIcon.Trash,
                SettingsWindowOption.Changelog => FontAwesomeIcon.List,
                SettingsWindowOption.PluginInformationAndDependencies => FontAwesomeIcon.InfoCircle,
                SettingsWindowOption.DeveloperToolbox => FontAwesomeIcon.Code,
                _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
            };
        }

        public bool IsDrawable()
        {
            return option switch
            {
                SettingsWindowOption.DeveloperToolbox => AWC.Config.DevMode,
                _ => true
            };
        }

        public void Draw()
        {
            switch (option) {
                case SettingsWindowOption.GeneralOptions:
                    GeneralOptionsUi.Draw();
                    break;
                case SettingsWindowOption.DutyOptions:
                    DutyOptionsUi.Draw();
                    break;
                case SettingsWindowOption.Characters:
                    CharactersOptionUi.Draw();
                    break;
                case SettingsWindowOption.RunnerOptions:
                    RunnerPrerequisitesUi.Draw();
                    break;
                case SettingsWindowOption.StopOptions:
                    StopActionsUi.Draw();
                    break;
                case SettingsWindowOption.ManuallyResetTomestone:
                    ResetWeeklyTomestonesUi.Draw();
                    break;
                case SettingsWindowOption.Changelog:
                    ChangelogUI.Draw();
                    break;
                case SettingsWindowOption.PluginInformationAndDependencies:
                    PluginInformationAndDependenciesUI.Draw();
                    break;
                case SettingsWindowOption.DeveloperToolbox:
                    DeveloperToolbox.Draw();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }
        }
    }
}
