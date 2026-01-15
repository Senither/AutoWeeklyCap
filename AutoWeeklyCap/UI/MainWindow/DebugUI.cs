using System;
using AutoWeeklyCap.Helpers;
using Dalamud.Bindings.ImGui;

namespace AutoWeeklyCap.UI.MainWindow;

public class DebugUI
{
    internal static void Draw()
    {
        ImGui.Text($"TaskManager [current task: {AutoWeeklyCap.TaskManager.CurrentTask?.Name ?? "idle"}]");

        ImGui.Separator();

        ImGui.Text($"Runner debug steps [stage: {AutoWeeklyCap.Runner.GetStatus()}]");
        DebugButton("Start", () => AutoWeeklyCap.Runner.Start(), false);
        DebugButton("Stop", () => AutoWeeklyCap.Runner.Stop());
        DebugButton("Resume", () => AutoWeeklyCap.Runner.Resume());
        DebugButton("Abort", () => AutoWeeklyCap.Runner.Abort());

        ImGui.Separator();

        ImGui.Text($"Task Helpers action buttons:");
        DebugButton("Extract", () => ExtractHelper.ExtractMateria(), false);
        DebugButton("Self Repair", () => RepairHelper.Repair());
        DebugButton("NPC Repair", () => RepairNPCHelper.Repair());
    }

    private static void DebugButton(string text, Action action, bool sameLine = true)
    {
        if (sameLine)
            ImGui.SameLine();

        if (ImGui.Button(text))
            action();
    }
}
