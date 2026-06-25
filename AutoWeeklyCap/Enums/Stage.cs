using AutoWeeklyCap.Contracts.Runner;
using AutoWeeklyCap.Runner.Stages;

namespace AutoWeeklyCap.Enums;

public enum Stage
{
    Waiting = 0,
    PreparingRunner = 1,
    WaitingForAutoRetainer = 2,
    CheckingTomestone = 3,
    StartingAutoDuty = 10,
    RunningAutoDuty = 11,
    StartingCharacterSwap = 20,
    SwitchingCharacter = 21,
    StoppingRunner = 999
}

public static class StateExtensions
{
    private static readonly WaitingStage Waiting = new();
    private static readonly PreparingRunnerStage PreparingRunner = new();
    private static readonly WaitForAutoRetainerStage WaitForAutoRetainer = new();
    private static readonly CheckTomestoneStage CheckTomestone = new();
    private static readonly StartAutoDutyStage StartAutoDuty = new();
    private static readonly RunAutoDutyStage RunAutoDuty = new();
    private static readonly StartCharacterSwapStage StartCharacterSwap = new();
    private static readonly SwitchingCharacterStage SwitchingCharacter = new();
    private static readonly StopRunnerStage StopRunner = new();

    extension(Stage stage)
    {
        public string? GetStatus(bool stopGracefully, string? currentCharacter)
        {
            return stage switch
            {
                Stage.Waiting => null,
                Stage.PreparingRunner => "Preparing runner",
                Stage.WaitingForAutoRetainer => "Waiting for AutoRetainer",
                Stage.CheckingTomestone => "Checking Tomestone",
                Stage.StartingAutoDuty => "Starting AutoDuty",
                Stage.RunningAutoDuty => stopGracefully ? "Stopping when duty finishes" : "Running AutoDuty",
                Stage.StartingCharacterSwap => "Starting Character Swap",
                Stage.SwitchingCharacter => "Switching Character to " + currentCharacter,
                Stage.StoppingRunner => "Stopping Runner",
                _ => "unknown"
            };
        }

        public string? GetStatusShort(bool stopGracefully, string? currentCharacter)
        {
            return stage switch
            {
                Stage.Waiting => "Off",
                Stage.SwitchingCharacter => "Switching Character",
                _ => stage.GetStatus(stopGracefully, currentCharacter)
            };
        }

        public BitmapFontIcon GetStatusIcon(bool stopGracefully)
        {
            return stage switch
            {
                Stage.Waiting => BitmapFontIcon.Away,
                Stage.PreparingRunner => BitmapFontIcon.FateCrafting,
                Stage.WaitingForAutoRetainer => BitmapFontIcon.Alarm,
                Stage.CheckingTomestone => BitmapFontIcon.OrangeDiamond,
                Stage.StartingAutoDuty => BitmapFontIcon.WaitingForDutyFinder,
                Stage.RunningAutoDuty => stopGracefully ? BitmapFontIcon.SwordSheathed : BitmapFontIcon.SwordUnsheathed,
                Stage.StartingCharacterSwap or Stage.SwitchingCharacter => BitmapFontIcon.WatchingCutscene,
                _ => BitmapFontIcon.Disconnecting
            };
        }

        public BaseStage GetStageInstance()
        {
            return stage switch
            {
                Stage.Waiting => Waiting,
                Stage.PreparingRunner => PreparingRunner,
                Stage.WaitingForAutoRetainer => WaitForAutoRetainer,
                Stage.CheckingTomestone => CheckTomestone,
                Stage.StartingAutoDuty => StartAutoDuty,
                Stage.RunningAutoDuty => RunAutoDuty,
                Stage.StartingCharacterSwap => StartCharacterSwap,
                Stage.SwitchingCharacter => SwitchingCharacter,
                Stage.StoppingRunner => StopRunner,
                _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
            };
        }
    }
}
