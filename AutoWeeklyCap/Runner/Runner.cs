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
            // @formatter:off
            AWC.TaskManager.Enqueue(() => {
                Svc.NotificationManager.AddNotification(new Notification {
                    Content = $"Auto start AWC in {seconds}!",
                    InitialDuration = TimeSpan.FromSeconds(1),
                    HardExpiry = DateTime.Now.AddSeconds(1),
                    Type = NotificationType.Warning
                });
            });
            // @formatter:on

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

    public bool Stop()
    {
        if (IsCurrentStageResumable() || !AutoDutyIPC.IsStopped()) {
            State.SetStoppingGracefully(true);
            return false;
        }

        Abort();

        return true;
    }

    public bool Resume()
    {
        if (!State.StoppingGracefully) {
            return false;
        }

        if (!IsCurrentStageResumable()) {
            return false;
        }

        State.SetStoppingGracefully(false);

        return true;
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

    private bool IsCurrentStageResumable()
    {
        return State.CurrentStage is Stage.RunningAutoDuty or Stage.SwitchingCharacter or Stage.WaitingForAutoRetainer;
    }
}
