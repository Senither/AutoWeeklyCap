using System.Collections.Generic;
using AutoWeeklyCap.Config;
using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace AutoWeeklyCap.UI.MainWindow;

internal static class CharactersUI
{
    internal static void Draw()
    {
        ImGui.BeginTable("Characters", 2);

        ImGui.TableNextColumn();
        ImGui.TableHeader(" Character");
        ImGui.TableNextColumn();
        ImGui.TableHeader(" Tomes");

        var charactersEnabled = 0;
        var totalTomesCollected = 0;
        var weeklyTomeLimit = InventoryManager.GetLimitedTomestoneWeeklyLimit();

        foreach (var (character, option) in Plugin.Config.Characters)
        {
            if (!option.Enabled)
                return;

            charactersEnabled++;
            var tomes = Plugin.Config.GetWeeklyTomes(character);
            totalTomesCollected += tomes;

            ImGui.TableNextColumn();
            ImGui.Text($" {character}");
            ImGui.TableNextColumn();
            ImGui.Text($" {tomes}/{weeklyTomeLimit}");
        }

        ImGui.EndTable();

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Spacing();

        ImGui.Text($"Weekly tomestone cap is at {totalTomesCollected}/{weeklyTomeLimit * charactersEnabled}");

        if (ImGui.Button("Reset Weekly Tomes"))
        {
            Plugin.Config.CollectedTomes.Clear();
            Plugin.Config.Save();
        }
    }
}
