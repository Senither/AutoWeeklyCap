using AutoWeeklyCap.Helpers;
using AutoWeeklyCap.IPC;
using ECommons;
using ECommons.ImGuiMethods;
using ECommons.Logging;

namespace AutoWeeklyCap.Commands;

public class RelogCommand : ICommand
{
    public string[] Triggers { get; } = ["relog", "r", "switch"];

    public string Description =>
        "Relogs to the specified character, the character must be a character that AWC already knows about, and must include the full name and homeworld name.";

    public void Run(string[] args)
    {
        if (!LifestreamIPC.IsEnabled)
        {
            DuoLog.Warning("Lifestream is not enabled");
            return;
        }

        if (LifestreamIPC.IsBusy())
        {
            DuoLog.Warning("Lifestream is already busy, can't relog to character");
            return;
        }

        if (AutoWeeklyCap.Runner.IsRunning() || AutoWeeklyCap.TaskManager.IsBusy)
        {
            DuoLog.Warning("AutoWeeklyCap is busy, can't relog to character");
            return;
        }

        var characterAndWorld = args.Join(" ");
        if (!AutoWeeklyCap.Config.Characters.ContainsKey(characterAndWorld))
        {
            DuoLog.Warning($"Unknown character '{characterAndWorld}', please specify a valid character");
            return;
        }

        if (characterAndWorld.Equals(Utils.GetFullCharacterName()))
            return;

        var parts = characterAndWorld.Split("@");
        if (parts.Length == 2)
        {
            LifestreamIPC.ChangeCharacter(parts[0], parts[1]);
        }
    }
}
