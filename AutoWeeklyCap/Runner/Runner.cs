using AutoWeeklyCap.IPC.AutoDuty;

using Dalamud.Interface.ImGuiNotification;

namespace AutoWeeklyCap.Runner;

public class Runner
{
    public RunnerState State { get; } = new();

    public bool Start()
    {
        if (State.CurrentStage != Stage.Waiting || State.StoppingGracefully) {
            return false;
        }

        if (!AWC.Config.IsRequiredSettingsSetup()) {
            return false;
        }

        string? character = PlayerHelper.GetFullCharacterName();
        if (character == null || !(AWC.Config.GetOrRegisterCharacterOptions(character)?.IsEnabled() ?? false)) {
            character = null;
        }

        State.SetCurrentCharacter(character);
        State.UpdateTimestamp();
        State.ResetRunsTrackers();

        bool usingAutoRetainer = AWC.Config.AutoRetainerEnabled && AutoRetainerIPC.IsEnabled;
        if (usingAutoRetainer && !AWC.Config.AutoRetainerTrigger.IsWithinThreshold()) {
            AutoRetainerIPC.DisableMultiMode();
        }

        State.ChangeStageTo(
            usingAutoRetainer && AWC.Config.AutoRetainerTrigger.IsWithinThreshold()
                ? Stage.WaitingForAutoRetainer
                : character != null && CurrencyHelper.IsPlayerLimitedTomestoneCapped()
                    ? Stage.StartingCharacterSwap
                    : Stage.PreparingRunner
        );

        if (AWC.Config.MuteGameSoundsWhenRunning) {
            AudioHelper.MuteMasterGameAudio(true);
        }

        AWC.Log.Info("Starting weekly cap runner");

        return true;
    }

    public bool AutoStartOnBoot()
    {
        if (State.CurrentStage != Stage.Waiting || State.StoppingGracefully) {
            return false;
        }

        if (!AWC.Config.IsRequiredSettingsSetup()) {
            return false;
        }

        if (PlayerHelper.IsValid) {
            return false;
        }

        if (AWC.Config.GetFirstUncappedCharacter() == null) {
            return false;
        }

        const int autoStartDelay = 5;
        for (var i = 0; i < autoStartDelay; i++) {
            int seconds = autoStartDelay - i;
            AWC.TaskManager.Enqueue(() => Svc.NotificationManager.AddNotification(new Notification { Content = $"Auto start AWC in {seconds}!", InitialDuration = TimeSpan.FromSeconds(1), HardExpiry = DateTime.Now.AddSeconds(1), Type = NotificationType.Warning }));

            AWC.TaskManager.EnqueueDelay(1000);
        }

        AWC.TaskManager.Enqueue(() =>
        {
            State.ChangeStageTo(Stage.WaitingForAutoRetainer);
            State.UpdateTimestamp();
            State.ResetRunsTrackers();
            State.SetCurrentCharacter(AWC.Config.GetFirstUncappedCharacter());
        });

        if (AWC.Config.MuteGameSoundsWhenRunning) {
            AudioHelper.MuteMasterGameAudio(true);
        }

        return true;
    }

    public void Stop()
    {
        if (PlayerHelper.IsLoggedIn) {
            if (State.CurrentStage is Stage.RunningAutoDuty or Stage.SwitchingCharacter || !AutoDutyIPC.IsStopped()) {
                State.SetStoppingGracefully(true);
                return;
            }
        }

        Abort();
    }

    public void Resume()
    {
        if (!State.StoppingGracefully || AutoDutyIPC.IsStopped()) {
            return;
        }

        if (State.CurrentStage is not (Stage.RunningAutoDuty or Stage.SwitchingCharacter)) {
            return;
        }

        State.SetStoppingGracefully(false);
    }

    public void Abort()
    {
        State.Reset();

        LifestreamIPC.Abort();
        AutoDutyIPC.Stop();
        TitleManager.Reset();
        LocationManager.Reset();
        AWC.TaskManager.Abort();

        if (AWC.Config.UseBossModRebornAI && BossModRebornIPC.IsEnabled) {
            BossModRebornIPC.DisableAI();
        }

        if (AWC.Config.UseAutoDutyProfileOverride) {
            AutoDutyProfile.Pop();
        }

        if (AWC.Config.MuteGameSoundsWhenRunning) {
            AudioHelper.MuteMasterGameAudio(false);
        }

        AWC.Log.Info("Stopped weekly cap runner");
    }

    public void Tick()
    {
        if (AWC.TaskManager.IsBusy) {
            return;
        }

        State.CurrentStage.GetStageInstance().Handle(this, State);
    }
}
