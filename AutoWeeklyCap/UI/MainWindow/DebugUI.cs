using System;
using AutoWeeklyCap.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace AutoWeeklyCap.UI.MainWindow;

public class DebugUI
{
    internal static void Draw()
    {
        ImGui.Text($"TaskManager [current task: {AutoWeeklyCap.TaskManager.CurrentTask?.Name ?? "idle"}]");
        ImGui.Text($"Currencies [weekly: {Utils.GetWeeklyAcquiredTomestoneCount()}, uncapped: {Utils.GetUncappedAcquiredTomestoneCount()}]");

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

        ImGui.Separator();
        ImGui.Text("Game data state:");
        ImGui.Text($"Position:  T:{Player.Territory.RowId} P:{Player.Position}");

        var taget = "<no target>";
        try
        {
            unsafe
            {
                var t = TargetSystem.Instance()->Target;

                taget = $"{t->GetName()} [id: {t->BaseId}]";
            }
        }
        catch (Exception)
        {
            // ignored
        }

        ImGui.Text($"Target:     {taget}");
        ImGui.Text("State:       ");
        StateText(() => PlayerHelper.IsReady, "Ready");
        StateText(() => PlayerHelper.IsValid, "Valid");
        StateText(() => PlayerHelper.IsOccupied, "Occupied");
        StateText(() => PlayerHelper.IsJumping, "Jumping");
        StateText(() => PlayerHelper.IsMoving, "Moving");
        StateText(() => PlayerHelper.IsCasting, "Casting", seperator: false);
    }

    private static void DebugButton(string text, Action action, bool sameLine = true)
    {
        if (sameLine)
            ImGui.SameLine();

        if (ImGui.Button(text))
            action();
    }

    private static void StateText(Func<bool> value, string text, bool seperator = true)
    {
        ImGui.SameLine(0, 0);


        try
        {
            ImGui.TextColored(value() ? ImGuiColors.HealerGreen : ImGuiColors.DPSRed, text);
        }
        catch (Exception)
        {
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);
        }

        if (seperator)
        {
            ImGui.SameLine(0, 0);
            ImGui.Text(" | ");
        }
    }
}
