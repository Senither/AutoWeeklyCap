using System;
using System.Linq;
using ECommons;
using ECommons.Logging;

namespace AutoWeeklyCap.Commands;

public static class CommandHandler
{
    internal static readonly ICommand[] Commands =
    [
        new StartCommand(),
        new StopCommand(),
        new RelogCommand(),
    ];

    public static void HandleCommand(string stringArgs)
    {
        var args = SplitArgs(stringArgs);
        if (args.Length == 0)
        {
            AutoWeeklyCap.Instance.ToggleMainUi();
            return;
        }

        var command = args[0].ToLower();
        if (command == "help")
        {
            PrintHelp(args.Skip(1).ToArray());
            return;
        }

        foreach (var cmd in Commands)
        {
            if (cmd.Triggers.Contains(command))
            {
                cmd.Run(args.Skip(1).ToArray());
                return;
            }
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
        if (args.Length == 0)
        {
            DuoLog.Information("AutoWeeklyCap Commands:");
            foreach (var cmd in Commands)
            {
                DuoLog.Information($" - {cmd.Triggers.Join(" / ")}");
            }

            DuoLog.Information($"Run with: '{AutoWeeklyCap.CommandNameShort} <option>'");
            return;
        }

        var command = args[0].ToLower();
        foreach (var cmd in Commands)
        {
            if (cmd.Triggers.Contains(command))
            {
                DuoLog.Information($"{cmd.Triggers[0]}  command:");
                DuoLog.Information($"{cmd.Description}");
                return;
            }
        }

        PrintUnknownCommand(command);
    }

    private static void PrintUnknownCommand(string command)
    {
        DuoLog.Information(
            $"Unknown command '{command}', use `{AutoWeeklyCap.CommandNameShort} help` for a list of commands");
    }
}
