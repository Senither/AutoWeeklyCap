using Dalamud.Game.Text.SeStringHandling;

namespace AutoWeeklyCap.Runner;

public enum State
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
    public static string GetStatus(this State state, bool stopGracefully, string? currentCharacter)
    {
        return state switch
        {
            State.Waiting => "idle",
            State.PreparingRunner => "Preparing runner",
            State.WaitingForAutoRetainer => "Waiting for AutoRetainer",
            State.CheckingTomestone => "Checking Tomestone",
            State.StartingAutoDuty => "Starting AutoDuty",
            State.RunningAutoDuty => stopGracefully ? "Stopping when duty finishes" : "Running AutoDuty",
            State.StartingCharacterSwap => "Starting Character Swap",
            State.SwitchingCharacter => "Switching Character to " + currentCharacter,
            State.StoppingRunner => "Stopping Runner",
            _ => "unknown"
        };
    }

    public static string GetStatusShort(this State state, bool stopGracefully, string? currentCharacter)
    {
        return state switch
        {
            State.Waiting => "Off",
            State.SwitchingCharacter => "Switching Character",
            _ => state.GetStatus(stopGracefully, currentCharacter)
        };
    }

    public static BitmapFontIcon GetStatusIcon(this State state, bool stopGracefully)
    {
        return state switch
        {
            State.Waiting => BitmapFontIcon.Away,
            State.PreparingRunner => BitmapFontIcon.FateCrafting,
            State.WaitingForAutoRetainer => BitmapFontIcon.Alarm,
            State.CheckingTomestone => BitmapFontIcon.OrangeDiamond,
            State.StartingAutoDuty => BitmapFontIcon.WaitingForDutyFinder,
            State.RunningAutoDuty => stopGracefully ? BitmapFontIcon.SwordSheathed : BitmapFontIcon.SwordUnsheathed,
            State.StartingCharacterSwap or State.SwitchingCharacter => BitmapFontIcon.WatchingCutscene,
            _ => BitmapFontIcon.Disconnecting,
        };
    }
}
