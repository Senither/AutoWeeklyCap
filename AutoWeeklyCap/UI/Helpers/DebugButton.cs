using AutoWeeklyCap.Contracts.Runner;

using ECommons.Logging;

using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoWeeklyCap.UI.Helpers;

public static class DebugButton
{
    public unsafe delegate string? DebugAddonAction(AtkUnitBase* addon);

    public static void Draw(string text, Action action, bool sameLine = true)
    {
        if (sameLine) {
            ImGui.SameLine();
        }

        if (ImGui.Button(text)) {
            action();
        }
    }

    public static void Draw(string text, BaseAction action, bool sameLine = true)
    {
        if (sameLine) {
            ImGui.SameLine();
        }

        if (ImGui.Button(text)) {
            DuoLog.Warning($"{action.GetName()}: {action.Invoke()}");
        }
    }

    public static unsafe void Draw(string addonName, DebugAddonAction action, bool sameLine = true)
    {
        if (sameLine) {
            ImGui.SameLine();
        }

        if (!ImGui.Button(addonName)) {
            return;
        }

        try {
            if (!AddonHelper.TryGetReadyAddon(addonName, out var addon)) {
                DuoLog.Warning($"Found no addon \"{addonName}\" that are ready");
                return;
            }

            var result = action.Invoke(addon);
            if (result != null) {
                ImGui.SetClipboardText(result);
                DuoLog.Warning($"Copied {addonName} addon content to clipboard");
            } else {
                DuoLog.Warning($"{addonName} returned null");
            }
        } catch (Exception ex) {
            AWC.Log.Error($"Got error while running debug addon action for {addonName}", ex);
        }
    }
}
