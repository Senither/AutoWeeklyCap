using System;
using AutoWeeklyCap.Helpers;
using AutoWeeklyCap.IPC;

namespace AutoWeeklyCap.Actions;

public enum StopAction
{
    None = 0,
    SwitchCharacter = 1,
    LogoutToMenu = 2,
    ShutdownGame = 3,
}

public static class StopActionExtensions
{
    public static string GetName(this StopAction action)
    {
        return action switch
        {
            StopAction.None => "Nothing",
            StopAction.SwitchCharacter => "Switch to Character",
            StopAction.LogoutToMenu => "Logout to Menu",
            StopAction.ShutdownGame => "Shutdown Game",
            _ => action.ToString()
        };
    }

    public static void Execute(this StopAction action)
    {
        AutoWeeklyCap.Log.Debug($"Executing action: {action.GetName()}");

        switch (action)
        {
            case StopAction.None:
                break;

            case StopAction.SwitchCharacter:
                if (AutoWeeklyCap.Config.CharacterForSwap.Length > 0)
                {
                    var parts = AutoWeeklyCap.Config.CharacterForSwap.Split("@");
                    if (parts.Length == 2)
                    {
                        LifestreamIPC.ChangeCharacter(parts[0], parts[1]);
                    }
                }
                break;

            case StopAction.LogoutToMenu:
                var status = LifestreamIPC.Logout();
                AutoWeeklyCap.Log.Debug($"Logging out via Lifestream with status: {status}");
                break;

            case StopAction.ShutdownGame:
                Utils.RunShellCommand("xlkill");
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}
