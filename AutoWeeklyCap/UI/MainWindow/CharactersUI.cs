using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

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

        var charactersFound = 0;
        var totalTomesCollected = 0;
        var weeklyTomeLimit = InventoryManager.GetLimitedTomestoneWeeklyLimit();

        foreach (var character in Plugin.Config.Characters)
        {
            if (character.Length == 0)
                continue;

            charactersFound++;
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

        ImGui.Text($"Weekly tomestone cap is at {totalTomesCollected}/{weeklyTomeLimit * charactersFound}");

        if (ImGui.Button("Reset Weekly Tomes"))
        {
            Plugin.Config.CollectedTomes.Clear();
            Plugin.Config.Save();
        }
    }
}
