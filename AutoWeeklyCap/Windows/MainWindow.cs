using System;
using System.Numerics;
using AutoWeeklyCap.IPC;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace AutoWeeklyCap.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin) : base("Auto Weekly Tomestone Capper##main-window")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(460, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.BeginTable("header-options", 2);

        ImGui.TableNextColumn();
        ImGui.TextColored(ImGuiColors.HealerGreen, LifestreamIPC.IsEnabled ? "✓ Lifestream" : "✖ Lifestream");
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.HealerGreen, AutoDutyIPC.IsEnabled ? "✓ AutoDuty" : "✖ AutoDuty");

        ImGui.TableNextColumn();
        if (Plugin.Runner.IsRunning())
        {
            if (ImGui.Button(" Stop "))
            {
                Plugin.Runner.Stop();
            }
        }
        else
        {
            if (ImGui.Button(" Start "))
            {
                if (LifestreamIPC.IsEnabled && AutoDutyIPC.IsEnabled)
                {
                    Plugin.Runner.Start();
                }
            }
        }


        ImGui.SameLine();
        if (ImGui.Button(" Manage Characters "))
            plugin.ToggleConfigUi();

        ImGui.EndTable();
        
        ImGui.Text("Status: " + Plugin.Runner.GetStatus());

        using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
        {
            if (child.Success)
            {
                if (Plugin.DataManager.GetExcelSheet<TerritoryType>()
                          .TryGetRow(plugin.Configuration.ZoneId, out var territoryRow))
                {
                    ImGui.Text(
                        $"Selected duty is {territoryRow.PlaceName.Value.Name} ({plugin.Configuration.ZoneId})");
                }
                else
                {
                    ImGui.Text("Enter a valid zone ID in the settings");
                }
            }

            ImGui.Spacing();
            ImGui.Spacing();

            ImGui.BeginTable("Characters", 2);

            ImGui.TableNextColumn();
            ImGui.TableHeader(" Character");
            ImGui.TableNextColumn();
            ImGui.TableHeader(" Tomes");

            var charactersFound = 0;
            var totalTomesCollected = 0;
            var weeklyTomeLimit = InventoryManager.GetLimitedTomestoneWeeklyLimit();

            foreach (var character in plugin.Configuration.Characters)
            {
                if (character.Length == 0)
                    continue;

                charactersFound++;
                var tomes = plugin.Configuration.GetWeeklyTomes(character);
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
                plugin.Configuration.CollectedTomes.Clear();
                plugin.Configuration.Save();
            }
        }
    }
}
