using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.Enums;

public enum StopAction
{
    None = 0,
    SwitchCharacter = 1,
    LogoutToMenu = 2,
    ShutdownGame = 3,
    AutoRetainerMultimode = 4,
    StartUnlimitedRuns = 5,
    LevelJobs = 6
}

public static class StopActionExtensions
{
    public static List<StopAction> GetOrderedStopActions()
    {
        return
        [
            StopAction.None,
            StopAction.LogoutToMenu,
            StopAction.ShutdownGame,
            StopAction.SwitchCharacter,
            StopAction.AutoRetainerMultimode,
            StopAction.LevelJobs,
            StopAction.StartUnlimitedRuns,
        ];
    }

    extension(StopAction action)
    {
        public string GetName()
        {
            return action switch
            {
                StopAction.None => "Nothing",
                StopAction.SwitchCharacter => "Switch to Character",
                StopAction.LogoutToMenu => "Logout to Menu",
                StopAction.ShutdownGame => "Shutdown Game",
                StopAction.AutoRetainerMultimode => "Start AutoRetainer multimode",
                StopAction.StartUnlimitedRuns => "Start Unlimited Runs",
                StopAction.LevelJobs => "Level Jobs & Alts",
                _ => action.ToString()
            };
        }

        public string GetRunnerStatusText()
        {
            return action switch
            {
                StopAction.StartUnlimitedRuns => "Runner is preforming unlimited runs",
                StopAction.LevelJobs => "Runner is leveling your characters",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };
        }

        public string? GetActionButtonText()
        {
            return action switch
            {
                StopAction.StartUnlimitedRuns => "Start Unlimited Runs",
                StopAction.LevelJobs => "Start Leveling Jobs",
                _ => null
            };
        }

        public Action? GetTooltip()
        {
            return action switch
            {
                StopAction.SwitchCharacter => () => ImGui.Text("If the runner finished on your preferred character, nothing will happen"),
                StopAction.AutoRetainerMultimode => () =>
                {
                    ImGui.Text("This requires ");
                    StatusText.Draw(AutoRetainerIPC.IsEnabled, "AutoRetainer");
                    ImGui.Text(" to be enabled, if it's not enabled it will do nothing");
                },
                StopAction.StartUnlimitedRuns => () => ImGui.Text(
                    "When the runner finishes capping all your characters it will switch to your\n" +
                    "preferred character and then start doing runs until manually stopped"
                ),
                StopAction.LevelJobs => () => ImGui.Text(
                    "When the runner finishes capping all your characters, it will switch to your\n" +
                    "selected job to level instead, and continue doing runs on that"
                ),
                _ => null
            };
        }

        public void Execute()
        {
            AWC.Log.Debug($"StopAction: Executing action: {action.GetName()}");

            switch (action) {
                case StopAction.None:
                    break;

                case StopAction.SwitchCharacter:
                    var characterToSwapTo = AWC.Config.CharacterForSwap;
                    if (characterToSwapTo.Length == 0 || characterToSwapTo == PlayerHelper.GetFullCharacterName()) {
                        break;
                    }

                    var parts = characterToSwapTo.Split("@");
                    if (parts.Length == 2) {
                        LifestreamIPC.ChangeCharacter(parts[0], parts[1]);
                    }

                    break;

                case StopAction.LogoutToMenu:
                    var status = LifestreamIPC.Logout();
                    AWC.Log.Debug($"StopAction: Logging out via Lifestream with status: {status}");
                    break;

                case StopAction.ShutdownGame:
                    ChatHelper.RunCommand("xlkill");
                    break;

                case StopAction.AutoRetainerMultimode:
                    AutoRetainerIPC.EnableMultiMode();
                    break;

                case StopAction.StartUnlimitedRuns:
                case StopAction.LevelJobs:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        public void StartRunnerAsAction()
        {
            if (AWC.Runner.IsRunning()) {
                return;
            }

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (action) {
                case StopAction.StartUnlimitedRuns: {
                    if (AWC.Runner.Start()) {
                        AWC.Runner.ForceEnableUnlimitedMode();
                    }

                    break;
                }

                case StopAction.LevelJobs: {
                    if (AWC.Runner.Start()) {
                        AWC.Runner.ForceEnableLevelingMode();
                    }

                    break;
                }
            }
        }
    }
}
