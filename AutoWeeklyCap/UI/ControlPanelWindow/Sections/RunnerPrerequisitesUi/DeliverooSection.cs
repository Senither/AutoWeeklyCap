using AutoWeeklyCap.UI.Helpers;

using Range = AutoWeeklyCap.UI.Helpers.Range;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.RunnerPrerequisitesUi;

internal static class DeliverooSection
{
    internal static void Draw()
    {
        ImGui.Spacing();

        bool useDeliveroo = AWC.Config.DeliverooEnabled;
        if (ImGui.Checkbox("Use Deliveroo", ref useDeliveroo)) {
            AWC.Config.DeliverooEnabled = useDeliveroo;
        }

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

        PluginSettingsButton.Draw(DeliverooIPC.Context);

        Disabled.Draw(!AWC.Config.DeliverooEnabled, () =>
        {
            ImGui.Spacing();

            ImGui.Text("When should Deliveroo be used?");

            if (ImGui.RadioButton("After", AWC.Config.DeliverooOnInterval)) {
                AWC.Config.DeliverooOnInterval = true;
            }

            ImGui.SameLine();
            ImGui.SetNextItemWidth(80 * ImGuiHelpers.GlobalScale);

            uint configDeliverooRunInterval = AWC.Config.DeliverooRunInterval;
            if (Range.Draw("runs###deliveroo-run-interval", ref configDeliverooRunInterval, 1, 10)) {
                AWC.Config.DeliverooRunInterval = configDeliverooRunInterval;
            }

            InformationTooltip.Draw(() =>
            {
                ImGui.Text("Runs are only counted on a per-character basis, switching");
                ImGui.Text("between characters will reset the runs counter");
            });

            if (ImGui.RadioButton("After character is tomestone capped", !AWC.Config.DeliverooOnInterval)) {
                AWC.Config.DeliverooOnInterval = false;
            }

            ImGui.Spacing();

            ImGui.Text("How should Deliveroo handle items with materia?");

            DeliverooStuckAction selectedAction = AWC.Config.DeliverooStuckAction;
            foreach (DeliverooStuckAction action in Enum.GetValues<DeliverooStuckAction>()) {
                if (ImGui.RadioButton(action.GetName(), selectedAction == action)) {
                    AWC.Config.DeliverooStuckAction = action;
                }

                InformationTooltip.Draw(action.GetTooltip());
            }

            Card.Separator();
            ImGui.Spacing();

            bool runOnFirstLoop = AWC.Config.DeliverooRunOnFirstLoop;
            if (ImGui.Checkbox("Always run before the first AutoDuty run", ref runOnFirstLoop)) {
                AWC.Config.DeliverooRunOnFirstLoop = runOnFirstLoop;
            }
        });
    }
}
