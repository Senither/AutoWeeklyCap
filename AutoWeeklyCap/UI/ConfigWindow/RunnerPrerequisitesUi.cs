using System;
using AutoWeeklyCap.Helpers;
using AutoWeeklyCap.IPC;
using AutoWeeklyCap.UI.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using ECommons.ImGuiMethods;
using Range = AutoWeeklyCap.UI.Helpers.Range;

namespace AutoWeeklyCap.UI.ConfigWindow;

public static class RunnerPrerequisitesUi
{
    public static void Draw()
    {
        ImGui.Text("Select what should happen before and between runs.");
        ImGui.Spacing();

        Card.DrawSubtle("General Options###runner-prereq-general", DrawGeneralOptions, defaultOpen: true);
        Card.DrawSubtle("Auto Retainer###runner-prereq-auto-retainer", DrawAutoRetainer);
        Card.DrawSubtle("Deliveroo###runner-prereq-deliveroo", DrawDeliveroo);
    }

    private static void DrawGeneralOptions()
    {
        ImGui.Spacing();

        // Repair (Self & NPC)
        var repairStatus = AutoWeeklyCap.Config.Repair;
        if (ImGui.Checkbox("Repair Gear", ref repairStatus))
            AutoWeeklyCap.Config.Repair = repairStatus;

        Disabled.Draw(!AutoWeeklyCap.Config.Repair, () =>
        {
            ImGui.SameLine();
            if (ImGui.RadioButton("Self", AutoWeeklyCap.Config.RepairSelf))
                AutoWeeklyCap.Config.RepairSelf = true;

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("Will use Dark Matter to Self Repair (Requires Leveled Crafters!)");
                ImGui.Text("If self repair is not possible NPC repairs will be used instead");
            });

            ImGui.SameLine();
            if (ImGui.RadioButton("City NPC", !AutoWeeklyCap.Config.RepairSelf))
                AutoWeeklyCap.Config.RepairSelf = false;

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("Will teleport to your grand company and use gil to repair your gear");

                ImGui.Text("Requires ");
                StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
                ImGui.Text(" and ");
                StatusText.Draw(VNavMeshIPC.IsEnabled, "VNavMesh");
                ImGui.Text(" to be enabled");
            });

            ImGui.Text("Trigger @");
            ImGui.SameLine();

            var width = (int)Math.Max(150, ImGui.GetContentRegionAvail().X / 1.5);
            ImGui.PushItemWidth(width * ImGuiHelpers.GlobalScale);

            var autoRepairPercentage = AutoWeeklyCap.Config.RepairPercentage;
            if (Range.Draw("##Repair@", ref autoRepairPercentage, 1, 99, "%d%%"))
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
        var autoSpendUncappedTomestones = AutoWeeklyCap.Config.SpendUncappedTomestones;
        if (ImGui.Checkbox("Auto Spend Uncapped Tomestones", ref autoSpendUncappedTomestones))
            AutoWeeklyCap.Config.SpendUncappedTomestones = autoSpendUncappedTomestones;

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Will teleport to the Nexus Arcade in Solution Nine and buy your");
            ImGui.Text("selected items from Zircon with your uncapped tomestones");

            ImGui.Text("Requires ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" and ");
            StatusText.Draw(VNavMeshIPC.IsEnabled, "VNavMesh");
            ImGui.Text(" to be enabled");
        });

        Disabled.Draw(!AutoWeeklyCap.Config.SpendUncappedTomestones, () =>
        {
            ImGui.Text("Buy @");
            ImGui.SameLine();

            var width = (int)Math.Max(150, ImGui.GetContentRegionAvail().X / 1.5);
            ImGui.PushItemWidth(width * ImGuiHelpers.GlobalScale);

            var autoBuyWithUncappedTomestones = AutoWeeklyCap.Config.SpendUncappedTomestoneThreshold;
            if (Range.Draw("##BuyTomestones@", ref autoBuyWithUncappedTomestones, 1, 2000))
                AutoWeeklyCap.Config.SpendUncappedTomestoneThreshold = autoBuyWithUncappedTomestones;

            ImGui.PopItemWidth();

            ImGui.Text("Item to buy");
            var selectedItem = TomestoneItemHelper.GetTomestoneItemFromName(AutoWeeklyCap.Config.SpendUncappedTomestoneItemName);
            if (ImGui.BeginCombo("##PreferredUncappedTomestoneItem", selectedItem != null ? selectedItem.Name : "Not selected"))
            {
                foreach (var item in TomestoneItemHelper.GetTomestoneItems())
                {
                    if (ImGui.Selectable(item.Name))
                        AutoWeeklyCap.Config.SpendUncappedTomestoneItemName = item.Name;
                }

                ImGui.EndCombo();
            }
        });
    }

    private static void DrawAutoRetainer()
    {
        ImGui.Spacing();

        var useAutoRetainer = AutoWeeklyCap.Config.AutoRetainerEnabled;
        if (ImGui.Checkbox("Use Auto Retainer", ref useAutoRetainer))
            AutoWeeklyCap.Config.AutoRetainerEnabled = useAutoRetainer;

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled and at least one retainer are ready within your selected threshold, the runner will ");
            ImGui.Text("enable MultiMode and then do one full cycle on all your characters before doing another run");

            ImGui.Text("Requires ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" and ");
            StatusText.Draw(AutoRetainerIPC.IsEnabled, "AutoRetainer");
        });

        Disabled.Draw(!AutoWeeklyCap.Config.AutoRetainerEnabled, () =>
        {
            var width = (int)Math.Max(150, ImGui.GetContentRegionAvail().X / 2.5);
            ImGui.PushItemWidth(width * ImGuiHelpers.GlobalScale);

            ImGui.Text("Wait for up to");
            ImGui.SameLine();

            var autoRetainerRemainingTime = AutoWeeklyCap.Config.AutoRetainerThreshold;
            if (Range.Draw("###AutoRetainerTimeWaitingRange", ref autoRetainerRemainingTime, 0, 300))
                AutoWeeklyCap.Config.AutoRetainerThreshold = autoRetainerRemainingTime;

            ImGui.SameLine();
            ImGui.Text("seconds");

            ImGui.PopItemWidth();
        });
    }

    private static void DrawDeliveroo()
    {
        ImGui.Spacing();

        var useDeliveroo = AutoWeeklyCap.Config.DeliverooEnabled;
        if (ImGui.Checkbox("Use Deliveroo", ref useDeliveroo))
            AutoWeeklyCap.Config.DeliverooEnabled = useDeliveroo;

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled the runner will move to your grand company and trade in all unused and tradable");
            ImGui.Text("items for grand company seals, and then buy your preferred items (setup within Deliveroo)");

            ImGui.Text("Requires ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" and ");
            StatusText.Draw(VNavMeshIPC.IsEnabled, "VNavMesh");
            ImGui.Text(" to be enabled, along with ");
            StatusText.Draw(DeliverooIPC.IsEnabled, "Deliveroo");
        });

        Disabled.Draw(!AutoWeeklyCap.Config.DeliverooEnabled, () =>
        {
            ImGui.Text("When should Deliveroo be used?");

            if (ImGui.RadioButton("After", AutoWeeklyCap.Config.DeliverooOnInterval))
                AutoWeeklyCap.Config.DeliverooOnInterval = true;

            ImGui.SameLine();
            ImGui.PushItemWidth(80 * ImGuiHelpers.GlobalScale);

            var configDeliverooRunInterval = AutoWeeklyCap.Config.DeliverooRunInterval;
            if (Range.Draw("runs###deliveroo-run-interval", ref configDeliverooRunInterval, 1, 10))
                AutoWeeklyCap.Config.DeliverooRunInterval = configDeliverooRunInterval;

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("Runs are only counted on a per-character basis, switching");
                ImGui.Text("between characters will reset the runs counter");
            });

            ImGui.PopItemWidth();

            if (ImGui.RadioButton("After character is tomestone capped", !AutoWeeklyCap.Config.DeliverooOnInterval))
                AutoWeeklyCap.Config.DeliverooOnInterval = false;
        });
    }
}
