using System;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using ECommons.ImGuiMethods;

namespace AutoWeeklyCap.UI.ConfigWindow;

public static class RunnerPrerequisitesUi
{
    public static void Draw()
    {
        ImGui.Text("Select what should happen before and between runs.");
        Card.Separator();

        Disabled.Draw(false, DrawGeneralOptions);
        Card.Separator();

        Disabled.Draw(true, DrawAutoRetainer);
    }

    private static void DrawGeneralOptions()
    {
        ImGuiEx.TextCentered(ColorUtils.HexToVector(0x59, 0x69, 0xFF), "General Options");

        // Repair (Self & NPC)
        var repairStatus = AutoWeeklyCap.Config.Repair;
        if (ImGui.Checkbox("Repair Gear", ref repairStatus))
            AutoWeeklyCap.Config.Repair = repairStatus;

        Disabled.Draw(!AutoWeeklyCap.Config.Repair, () =>
        {
            ImGui.SameLine();
            if (ImGui.RadioButton("Self", AutoWeeklyCap.Config.RepairSelf))
                AutoWeeklyCap.Config.RepairSelf = true;

            InformationTooltip.Draw("Will use Dark Matter to Self Repair (Requires Leveled Crafters!)");

            Disabled.Draw(() =>
            {
                ImGui.SameLine();
                if (ImGui.RadioButton("City NPC", !AutoWeeklyCap.Config.RepairSelf))
                    AutoWeeklyCap.Config.RepairSelf = false;

                InformationTooltip.Draw(
                    "City NPC repairs are currently still in development, it will be implemented in the future");
            });

            ImGui.Text("Trigger @");
            ImGui.SameLine();

            var width = (int)Math.Max(150, ImGui.GetContentRegionAvail().X / 1.5);
            ImGui.PushItemWidth(width * ImGuiHelpers.GlobalScale);

            var autoRepairPercentage = AutoWeeklyCap.Config.RepairPercentage;
            if (ImGui.SliderUInt("##Repair@", ref autoRepairPercentage, 1, 99, "%d%%"))
                AutoWeeklyCap.Config.RepairPercentage = Math.Min(100, Math.Max(1, autoRepairPercentage));

            ImGui.PopItemWidth();

            ImGui.Spacing();
            ImGui.Spacing();
        });

        // Auto Extract
        var autoExtract = AutoWeeklyCap.Config.Extract;
        if (ImGui.Checkbox("Extract Materia", ref autoExtract))
            AutoWeeklyCap.Config.Extract = autoExtract;

        Disabled.Draw(!AutoWeeklyCap.Config.Extract, () =>
        {
            ImGui.SameLine(0, 10);
            if (ImGui.RadioButton("Equipped", !AutoWeeklyCap.Config.ExtractAll))
                AutoWeeklyCap.Config.ExtractAll = false;

            ImGui.SameLine(0, 5);
            if (ImGui.RadioButton("All", AutoWeeklyCap.Config.ExtractAll))
                AutoWeeklyCap.Config.ExtractAll = true;
        });

        // Auto Spend Tomestones
        Disabled.Draw(() =>
        {
            var autoSpendUncappedTomestones = AutoWeeklyCap.Config.SpendUncappedTomestones;
            if (ImGui.Checkbox("Auto Spend Uncapped Tomestones", ref autoSpendUncappedTomestones))
                AutoWeeklyCap.Config.SpendUncappedTomestones = autoSpendUncappedTomestones;

            Disabled.Draw(!AutoWeeklyCap.Config.SpendUncappedTomestones, () =>
            {
                ImGui.Text("Buy @");
                ImGui.SameLine();

                var width = (int)Math.Max(150, ImGui.GetContentRegionAvail().X / 1.5);
                ImGui.PushItemWidth(width * ImGuiHelpers.GlobalScale);

                var autoBuyWithUncappedTomestones = AutoWeeklyCap.Config.SpendUncappedTomestoneThreshold;
                if (ImGui.SliderUInt("##BuyTomestones@", ref autoBuyWithUncappedTomestones, 1, 2000))
                    AutoWeeklyCap.Config.SpendUncappedTomestoneThreshold = autoBuyWithUncappedTomestones;

                ImGui.Text("Item to buy");
                if (ImGui.BeginCombo("##PreferredUncappedTomestoneItem", "Turali Pigment"))
                {
                    if (ImGui.Selectable("Turali Pigment")) { }

                    if (ImGui.Selectable("Test #1")) { }

                    if (ImGui.Selectable("Test #2")) { }

                    if (ImGui.Selectable("Test #3")) { }

                    ImGui.EndCombo();
                }
            });
        });
    }

    private static void DrawAutoRetainer()
    {
        ImGuiEx.TextCentered(ColorUtils.HexToVector(0xFF, 0x73, 0x59), "Auto Retainer");

        var useAutoRetainer = AutoWeeklyCap.Config.AutoRetainerEnabled;
        if (ImGui.Checkbox("Use Auto Retainer", ref useAutoRetainer))
            AutoWeeklyCap.Config.AutoRetainerEnabled = useAutoRetainer;

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

        var autoRetainerRemainingTime = AutoWeeklyCap.Config.AutoRetainerThreshold;
        if (ImGui.SliderUInt("###AutoRetainerTimeWaitingSlider", ref autoRetainerRemainingTime, 0, 300))
            AutoWeeklyCap.Config.AutoRetainerThreshold = autoRetainerRemainingTime;

        ImGui.SameLine();
        ImGui.Text("seconds");

        ImGui.PopItemWidth();
    }
}
