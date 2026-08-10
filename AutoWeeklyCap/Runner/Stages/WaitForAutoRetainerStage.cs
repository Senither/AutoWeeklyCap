using AutoWeeklyCap.Contracts.Runner;
using AutoWeeklyCap.IPC.AutoRetainer;

using ECommons.Configuration;

namespace AutoWeeklyCap.Runner.Stages;

public class WaitForAutoRetainerStage : BaseStage
{
    protected override string Name => nameof(WaitForAutoRetainerStage);

    private readonly Dictionary<ulong, Dictionary<string, long>> _retainerEndingAt = new();

    public override void Handle(Runner runner, RunnerState state)
    {
        if (!AutoRetainerIPC.GetMultiModeStatus()) {
            AutoRetainerIPC.EnableMultiMode();
        }

        if (_retainerEndingAt.Count == 0) {
            UpdatePlayerRetainersEndingAt();
        }

        if (AutoRetainerIPC.IsBusy() || LifestreamIPC.IsBusy() || (!PlayerHelper.IsValid && !AddonHelper.IsTitleScreenReady())) {
            state.UpdateTimestamp();
            return;
        }

        int elapsed = (DateTime.UtcNow - state.Timestamp).Seconds;

        switch (PlayerHelper.IsValid) {
            case true when elapsed < 15:
            case false when elapsed < 5:
                return;
        }

        // From this point onwards we're assuming that AutoRetainer has completed its run, next we'll return the original player

        StorePlayerRetainersEndingAtToMetrics();

        if (state.StoppingGracefully) {
            runner.Abort();
            return;
        }

        if (state.CurrentCharacter == null) {
            AutoRetainerIPC.DisableMultiMode();
            state.ChangeStageTo(Stage.StartingCharacterSwap);
            return;
        }

        if (PlayerHelper.GetFullCharacterName() == state.CurrentCharacter) {
            state.ChangeStageTo(Stage.PreparingRunner);
            return;
        }

        int limit = CurrencyHelper.GetLimitedTomestoneWeeklyLimit();
        int tomes = AWC.Config.CollectedTomes.GetValueOrDefault(state.CurrentCharacter, 0);
        if (tomes == limit) {
            state.ChangeStageTo(Stage.StartingCharacterSwap);
            return;
        }

        string[] parts = state.CurrentCharacter.Split("@");
        LogInfo($"Switching character to {parts[0]} on {parts[1]}");

        state.ChangeStageTo(Stage.SwitchingCharacter);
        state.UpdateTimestamp();

        LifestreamIPC.ChangeCharacter(parts[0], parts[1]);
    }

    private void UpdatePlayerRetainersEndingAt()
    {
        foreach (ulong id in AutoRetainerIPC.GetRegisteredCharacters()) {
            OfflineCharacterData characterData = AutoRetainerIPC.GetOfflineCharacterData(id);
            if (!characterData.Enabled) {
                continue;
            }

            var retainers = new Dictionary<string, long>();
            foreach (var retainer in characterData.RetainerData.Where(retainer => retainer.HasVenture)) {
                retainers[retainer.Name] = retainer.VentureEndsAt;
            }

            _retainerEndingAt[id] = retainers;
        }
    }

    private void StorePlayerRetainersEndingAtToMetrics()
    {
        foreach (var pair in _retainerEndingAt) {
            uint collectedRetainers = 0;

            OfflineCharacterData characterData = AutoRetainerIPC.GetOfflineCharacterData(pair.Key);

            foreach (var retainer in characterData.RetainerData) {
                if (!pair.Value.TryGetValue(retainer.Name, out var value)) {
                    continue;
                }

                if (retainer.VentureEndsAt != value) {
                    collectedRetainers++;
                }
            }

            AWC.Config.GetOrRegisterCharacterOptions(characterData.ToString())
                ?.Metrics
                .IncrementRetainersCollected(collectedRetainers);
        }

        _retainerEndingAt.Clear();
        EzConfig.Save();
    }
}
