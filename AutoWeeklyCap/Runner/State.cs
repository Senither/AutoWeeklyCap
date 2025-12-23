namespace AutoWeeklyCap.Runner;

public enum State
{
    Waiting = 0,
    StartingAutoDuty = 1,
    RunningAutoDuty = 2,
    StartingCharacterSwap = 3,
    SwitchingCharacter = 4,
    CheckingTomestone = 5
}
