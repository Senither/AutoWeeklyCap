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

        var character = PlayerHelper.GetFullCharacterName();
        if (character == null || !(AWC.Config.GetOrRegisterCharacterOptions(character)?.IsEnabled() ?? false)) {
            character = null;
        }

        State.SetCurrentCharacter(character);
        State.UpdateTimestamp();
        State.ResetRunsTrackers();

        State.ChangeStageTo(
            character != null && CurrencyHelper.IsPlayerLimitedTomestoneCapped()
                ? Stage.StartingCharacterSwap
                : Stage.PreparingRunner
        );

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
            var seconds = autoStartDelay - i;
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

        AWC.Log.Info("Stopped weekly cap runner");
    }

    public void Tick()
    {
        if (AWC.TaskManager.IsBusy) {
            return;
        }

        State.CurrentStage.Tick(this, State);
    }
}
