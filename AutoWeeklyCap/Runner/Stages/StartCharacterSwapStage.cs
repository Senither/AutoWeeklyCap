using AutoWeeklyCap.Contracts.Runner;

namespace AutoWeeklyCap.Runner.Stages;

public class StartCharacterSwapStage : BaseStage
{
    protected override string Name => nameof(StartCharacterSwapStage);

    public override void Handle(Runner runner, RunnerState state)
    {
        var character = AWC.Config.GetFirstUncappedCharacter();
        if (state.IsInNormalMode() && character != null) {
            if (ChangeCharacter(state, character)) {
                return;
            }

            runner.Stop();
            return;
        }

        if (state.RunsCounter > 0 && AWC.Config.NotificationMasterEnabled && AWC.Config.NotificationMasterUsingOnFullyCapped) {
            ActionInstance.Notification.ForceInvoke(StopNotificationType.CharacterCapped);
        }

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (AWC.Config.StopAction) {
            case StopAction.StartUnlimitedRuns:
                HandleUnlimitedMode(runner, state);
                break;
            case StopAction.LevelJobs:
                HandleLevelingMode(runner, state);
                break;

            default:
                LogInfo("Found no character with missing weekly capped tomestones, stopping runner");
                state.ChangeStageTo(Stage.StoppingRunner);
                break;
        }
    }

    private void HandleUnlimitedMode(Runner runner, RunnerState state)
    {
        state.EnableUnlimitedMode();
        LogInfo("All characters have been fully capped, starting unlimited runs");

        string preferredCharacter = AWC.Config.CharacterForSwap;
        if (PlayerHelper.GetFullCharacterName() == preferredCharacter) {
            LogDebug("Player is already on preferred character, starting runner");
            state.SetCurrentCharacter(preferredCharacter);
            state.ChangeStageTo(Stage.PreparingRunner);
            return;
        }

        if (ChangeCharacter(state, preferredCharacter)) {
            return;
        }

        runner.Stop();
    }

    private void HandleLevelingMode(Runner runner, RunnerState state)
    {
        state.EnableLevelingMode();

        string? levelableCharacter = LevelingHelper.GetCharacterToLevel();
        if (levelableCharacter == null) {
            LogDebug("Found no characters to level, stopping runner");
            runner.Stop();
            return;
        }

        if (PlayerHelper.GetFullCharacterName() == levelableCharacter) {
            LogDebug("Player is already on character to level, starting runner");
            state.SetCurrentCharacter(levelableCharacter);
            state.ChangeStageTo(Stage.PreparingRunner);
            return;
        }

        if (ChangeCharacter(state, levelableCharacter)) {
            return;
        }

        runner.Stop();
    }

    private bool ChangeCharacter(RunnerState state, string character)
    {
        string[] parts = character.Split("@");
        if (parts.Length != 2) {
            LogError($"Character {character} is not a valid character name, stopping runner");
            return false;
        }

        LogInfo($"Switching character to {parts[0]} on {parts[1]}");

        state.SetCurrentCharacter(character);
        state.ChangeStageTo(Stage.SwitchingCharacter);
        state.UpdateTimestamp();

        LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

        return true;
    }
}
