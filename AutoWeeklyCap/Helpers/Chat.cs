using System;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace AutoWeeklyCap.Helpers;

public static class Chat
{
    public static bool RunCommand(string commandString)
    {
        try
        {
            unsafe
            {
                var command = new Utf8String(
                    commandString.StartsWith('/')
                        ? commandString
                        : "/" + commandString
                );

                RaptureShellModule.Instance()->ExecuteCommandInner(
                    &command, UIModule.Instance());
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
