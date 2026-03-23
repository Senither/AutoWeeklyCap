using AutoWeeklyCap.UI.ConfigWindow;

using Dalamud.Interface;

namespace AutoWeeklyCap.Enums;

public enum SettingsWindowOption
{
    GeneralOptions = 0,
    DutyOptions = 1,
    Characters = 2,
    RunnerOptions = 3,
    StopOptions = 4,
    ManuallyResetTomestone = 5
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
                _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
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

                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }
        }
    }
}
