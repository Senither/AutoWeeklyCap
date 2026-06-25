using AutoWeeklyCap.Contracts.Commands;

using ECommons.Logging;

namespace AutoWeeklyCap.Commands;

public class RelogBaseCommand : BaseCommand
{
    public override string[] Triggers { get; } = ["relog", "r", "switch"];

    public override string Description =>
        "Relogs to the specified character, the character must be a character that AWC already knows about, and must include the full name and homeworld name.";

    public override void Run(string[] args)
    {
        if (!LifestreamIPC.IsEnabled) {
            DuoLog.Warning("Lifestream is not enabled");
            return;
        }

        if (LifestreamIPC.IsBusy()) {
            DuoLog.Warning("Lifestream is already busy, can't relog to character");
            return;
        }

        if (AWC.Runner.State.IsRunning() || AWC.TaskManager.IsBusy) {
            DuoLog.Warning("AutoWeeklyCap is busy, can't relog to character");
            return;
        }

        var characterAndWorld = args.Join(" ");
        if (!AWC.Config.Characters.ContainsKey(characterAndWorld)) {
            DuoLog.Warning($"Unknown character '{characterAndWorld}', please specify a valid character");
            return;
        }

        if (characterAndWorld.Equals(PlayerHelper.GetFullCharacterName())) {
            return;
        }

        var parts = characterAndWorld.Split("@");
        if (parts.Length != 2) {
            return;
        }

        AutoRetainerIPC.DisableMultiMode();
        LifestreamIPC.ChangeCharacter(parts[0], parts[1]);
    }
}
