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
