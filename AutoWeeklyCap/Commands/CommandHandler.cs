using AutoWeeklyCap.Contracts.Commands;

using ECommons.Logging;

namespace AutoWeeklyCap.Commands;

public static class CommandHandler
{
    private static readonly BaseCommand[] Commands =
    [
        new StartCommand(),
        new StopCommand(),
        new RelogCommand(),
        new DevModeCommand()
    ];

    public static void HandleCommand(string stringArgs)
    {
        var args = SplitArgs(stringArgs);
        if (args.Length == 0) {
            AWC.Instance.ToggleMainUi();
            return;
        }

        var command = args[0].ToLower();
        if (command == "help") {
            PrintHelp(args.Skip(1).ToArray());
            return;
        }

        foreach (var cmd in Commands) {
            if (!cmd.Triggers.Contains(command)) {
                continue;
            }

            cmd.Run(args.Skip(1).ToArray());
            return;
        }

        PrintUnknownCommand(command);
    }

    private static string[] SplitArgs(string input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? []
            : input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static void PrintHelp(string[] args)
    {
        if (args.Length == 0) {
            DuoLog.Information("AutoWeeklyCap Commands:");
            foreach (var cmd in Commands) {
                if (cmd.Hidden) {
                    continue;
                }

                DuoLog.Information($" - {cmd.Triggers.Join(" / ")}");
            }

            DuoLog.Information($"Run with: '{Constants.CommandNameShort} <option>'");
            return;
        }

        var command = args[0].ToLower();
        foreach (var cmd in Commands) {
            if (!cmd.Triggers.Contains(command)) {
                continue;
            }

            DuoLog.Information($"{cmd.Triggers[0]}  command:");
            DuoLog.Information($"{cmd.Description}");
            return;
        }

        PrintUnknownCommand(command);
    }

    private static void PrintUnknownCommand(string command)
    {
        DuoLog.Information($"Unknown command '{command}', use `{Constants.CommandNameShort} help` for a list of commands");
    }
}
