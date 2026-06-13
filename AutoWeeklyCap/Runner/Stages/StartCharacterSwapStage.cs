namespace AutoWeeklyCap.Runner.Stages;

public class StartCharacterSwapStage : BaseStage
{
    protected override string Name => nameof(StartCharacterSwapStage);

    public override void Handle(RunnerState state)
    {
        var character = AWC.Config.GetFirstUncappedCharacter();
        if (state.IsInNormalMode() && character != null) {
            var parts = character.Split("@");
            if (parts.Length != 2) {
                AWC.Log.Error($"Character {character} is not a valid character name, stopping runner");
                // TODO: call stop
                // Stop();
                return;
            }

            AWC.Log.Info($"Switching character to {parts[0]} on {parts[1]}");
            state.SetCurrentCharacter(character);
            state.ChangeStageTo(Stage.SwitchingCharacter);
            state.UpdateTimestamp();
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        if (state.RunsCounter > 0 && AWC.Config.NotificationMasterEnabled && AWC.Config.NotificationMasterUsingOnFullyCapped) {
            ActionInstance.Notification.ForceInvoke(StopNotificationType.CharacterCapped);
        }

        if (AWC.Config.StopAction == StopAction.StartUnlimitedRuns) {
            state.EnableUnlimitedMode();
            AWC.Log.Info("All characters have been fully capped, starting unlimited runs");

            var preferredCharacter = AWC.Config.CharacterForSwap;
            if (PlayerHelper.GetFullCharacterName() == preferredCharacter) {
                AWC.Log.Debug("Runner: Player is already on preferred character, starting runner");
                state.SetCurrentCharacter(preferredCharacter);
                state.ChangeStageTo(Stage.PreparingRunner);
                return;
            }

            var parts = preferredCharacter.Split("@");
            if (parts.Length != 2) {
                AWC.Log.Error($"Character {preferredCharacter} is not a valid character name, stopping runner");
                // TODO: call stop
                // Stop();
                return;
            }

            AWC.Log.Info($"Switching character to {parts[0]} on {parts[1]}");
            state.SetCurrentCharacter(preferredCharacter);
            state.ChangeStageTo(Stage.SwitchingCharacter);
            state.UpdateTimestamp();
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        if (AWC.Config.StopAction == StopAction.LevelJobs) {
            state.EnableLevelingMode();

            var levelableCharacter = LevelingHelper.GetCharacterToLevel();
            if (levelableCharacter == null) {
                AWC.Log.Debug($"Runner: Found no characters to level, stopping runner");
                // TODO: call stop
                // Stop();
                return;
            }

            if (PlayerHelper.GetFullCharacterName() == levelableCharacter) {
                AWC.Log.Debug("Runner: Player is already on character to level, starting runner");
                state.SetCurrentCharacter(levelableCharacter);
                state.ChangeStageTo(Stage.PreparingRunner);
                return;
            }

            var parts = levelableCharacter.Split("@");
            if (parts.Length != 2) {
                AWC.Log.Error($"Character {levelableCharacter} is not a valid character name, stopping runner");
                // TODO: call stop
                // Stop();
                return;
            }

            AWC.Log.Info($"Switching character to {parts[0]} on {parts[1]}");
            state.SetCurrentCharacter(levelableCharacter);
            state.ChangeStageTo(Stage.SwitchingCharacter);
            state.UpdateTimestamp();
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);

            return;
        }

        AWC.Log.Info("Found no character with missing weekly capped tomestones, stopping runner");
        state.ChangeStageTo(Stage.StoppingRunner);
    }
}
