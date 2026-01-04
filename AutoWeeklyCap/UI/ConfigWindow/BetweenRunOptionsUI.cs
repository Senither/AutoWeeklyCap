using System;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using ECommons.ImGuiMethods;

namespace AutoWeeklyCap.UI.ConfigWindow;

public static class BetweenRunOptionsUi
{
    public static void Draw()
    {
        Disabled.Draw(true, DrawGeneralOptions);

        Card.Separator();

        Disabled.Draw(true, DrawAutoRetainer);
    }

    private static void DrawGeneralOptions()
    {
        ImGuiEx.TextCentered(ColorUtils.HexToVector(0x59, 0x69, 0xFF), "General Options");

        var autoRepairStatus = false;
        ImGui.Checkbox("Auto Repair", ref autoRepairStatus);

        Disabled.Draw(!autoRepairStatus, () =>
        {
            ImGui.SameLine();
            if (ImGui.RadioButton("Self", true)) { }

            InformationTooltip.Draw("Will use DarkMatter to Self Repair (Requires Leveled Crafters!)");

            ImGui.SameLine();
            if (ImGui.RadioButton("City NPC", false)) { }

            InformationTooltip.Draw("Will use preferred repair npc to repair.");

            ImGui.Text("Trigger @");
            ImGui.SameLine();

            var width = (int)Math.Max(150, ImGui.GetContentRegionAvail().X / 1.5);
            ImGui.PushItemWidth(width * ImGuiHelpers.GlobalScale);

            var autoRepairPct = 80;
            if (ImGui.SliderInt("##Repair@", ref autoRepairPct, 1, 99, "%d%%")) { }

            ImGui.PopItemWidth();

            ImGui.Spacing();
            ImGui.Spacing();
        });

        var autoExtract = false;
        ImGui.Checkbox("Auto Extract", ref autoExtract);

        Disabled.Draw(!autoExtract, () =>
        {
            ImGui.SameLine(0, 10);
            if (ImGui.RadioButton("Equipped", true)) { }

            ImGui.SameLine(0, 5);
            if (ImGui.RadioButton("All", false)) { }
        });

        var autoSpendUncappedTomestones = false;
        ImGui.Checkbox("Auto Spend Uncapped Tomestones", ref autoSpendUncappedTomestones);

        Disabled.Draw(!autoSpendUncappedTomestones, () =>
        {
            ImGui.Text("Buy @");
            ImGui.SameLine();

            var width = (int)Math.Max(150, ImGui.GetContentRegionAvail().X / 1.5);
            ImGui.PushItemWidth(width * ImGuiHelpers.GlobalScale);

            var autoBuyWithUncappedTomestones = 1800;
            if (ImGui.SliderInt("##BuyTomestones@", ref autoBuyWithUncappedTomestones, 1, 2000)) { }

            ImGui.Text("Item to buy");
            if (ImGui.BeginCombo("##PreferredUncappedTomestoneItem", "Some nice item"))
            {
                if (ImGui.Selectable("Some nice item")) { }

                if (ImGui.Selectable("Test #1")) { }

                if (ImGui.Selectable("Test #2")) { }

                if (ImGui.Selectable("Test #3")) { }

                ImGui.EndCombo();
            }
        });
    }

    private static void DrawAutoRetainer()
    {
        ImGuiEx.TextCentered(ColorUtils.HexToVector(0xFF, 0x73, 0x59), "Auto Retainer");

        var useAutoRetainer = false;
        if (ImGui.Checkbox("Use Auto Retainer", ref useAutoRetainer))
        {
            // ...
        }

        ImGui.Text("Preferred summoning bell location:");
        InformationTooltip.Draw(
            "No matter what location is chosen, if there is a retainer bell"
            + "\nnear the location you're in, that bell will be used instead."
        );

        if (ImGui.BeginCombo("##PreferredBell", "Inn"))
        {
            if (ImGui.Selectable("Inn")) { }

            if (ImGui.Selectable("Test #1")) { }

            if (ImGui.Selectable("Test #2")) { }

            if (ImGui.Selectable("Test #3")) { }

            ImGui.EndCombo();
        }

        var width = (int)Math.Max(150, ImGui.GetContentRegionAvail().X / 2.5);
        ImGui.PushItemWidth(width * ImGuiHelpers.GlobalScale);

        ImGui.Text("Wait for up to");
        ImGui.SameLine();

        var autoRetainerRemainingTime = 90L;
        ImGui.SliderLong("###AutoRetainerTimeWaitingSlider", ref autoRetainerRemainingTime, 0L, 300L);

        ImGui.SameLine();
        ImGui.Text("seconds");

        ImGui.PopItemWidth();
    }
}
