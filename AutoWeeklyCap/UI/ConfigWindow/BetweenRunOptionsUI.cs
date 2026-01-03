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
        Disabled.Draw(true, DrawAutoRetainer);

        Card.Separator();

        ImGui.Text("Hi there");
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
