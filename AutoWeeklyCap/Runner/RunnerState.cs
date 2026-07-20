// ReSharper disable ArrangeMethodOrOperatorBody

namespace AutoWeeklyCap.Runner;

public class RunnerState
{
    public bool StoppingGracefully { get; private set; } = false;
    public bool UnlimitedMode { get; private set; } = false;
    public bool LevelingMode { get; private set; } = false;

    public Stage CurrentStage { get; private set; } = Stage.Waiting;
    public string? CurrentCharacter { get; private set; } = null;
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    public int RunsCounter { get; private set; } = 0;
    public string? RunsCharacter { get; private set; } = null;

    public bool UsingBossModRebornAi { get; private set; } = false;

    public DateTime? CurrentDutyStartUtc { get; private set; } = null;

    public void Reset()
    {
        StoppingGracefully = false;
        UnlimitedMode = false;
        LevelingMode = false;

        CurrentStage = Stage.Waiting;
        CurrentCharacter = null;
        Timestamp = DateTime.UtcNow;

        RunsCounter = 0;
        RunsCharacter = null;

        UsingBossModRebornAi = false;

        CurrentDutyStartUtc = null;
    }

    public bool IsRunning() => CurrentStage != Stage.Waiting;
    public bool IsInNormalMode() => !LevelingMode && !UnlimitedMode;

    public void EnableUnlimitedMode() => UnlimitedMode = true;
    public void EnableLevelingMode() => LevelingMode = true;

    public void IncrementRunsCounter() => RunsCounter++;
    public void UpdateTimestamp() => Timestamp = DateTime.UtcNow;

    public void SetRunsCounter(int counter) => RunsCounter = counter;
    public void SetCurrentCharacter(string? character) => CurrentCharacter = character;
    public void SetRunsCharacter(string? character) => RunsCharacter = character;
    public void SetStoppingGracefully(bool value) => StoppingGracefully = value;

    public void SetCurrentDutyStartUtc(DateTime? utc) => CurrentDutyStartUtc = utc;
    public void UpsertCurrentDutyStartUtc(DateTime? utc) => CurrentDutyStartUtc ??= utc;

    public void SetUsingBossModRebornAi(bool state) => UsingBossModRebornAi = state;

    public void ResetRunsTrackers()
    {
        RunsCounter = 0;
        RunsCharacter = null;
    }

    public void ChangeStageTo(Stage stage)
    {
        AWC.TaskManager.Enqueue(
            () => CurrentStage = stage,
            $"next stage: {stage}"
        );
    }
}
