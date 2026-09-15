using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.RunnerPrerequisitesUi;

internal static class BeforeRunOptionsSection
{
    internal static void Draw()
    {
        DrawReturnToHomeWorld();
        DrawStartAutoDutyFromSafezone();
    }

    private static void DrawReturnToHomeWorld()
    {
        bool returnToHomeWorld = AWC.Config.AlwaysStartOnHomeWorld;
        if (ImGui.Checkbox("Always return to home world", ref returnToHomeWorld)) {
            AWC.Config.AlwaysStartOnHomeWorld = returnToHomeWorld;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled and the runner has detected that your character is not on their");
            ImGui.Text("home world, it will use ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" to teleport back to your home world");
            ImGui.Text("before starting ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text(", ");
            StatusText.Draw(AutoRetainerIPC.IsEnabled, "AutoRetainer");
            ImGui.Text(", ");
            StatusText.Draw(DeliverooIPC.IsEnabled, "Deliveroo");
            ImGui.Text(", etc");
        });
    }

    private static void DrawStartAutoDutyFromSafezone()
    {
        bool onlyStartAutoDutyFromSafezone = AWC.Config.OnlyStartAutoDutyFromSafezone;
        if (ImGui.Checkbox("Only start AutoDuty from safezone", ref onlyStartAutoDutyFromSafezone)) {
            AWC.Config.OnlyStartAutoDutyFromSafezone = onlyStartAutoDutyFromSafezone;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Moves to your preferred safezone before starting ");
            StatusText.Draw(AutoDutyIPC.IsEnabled, "AutoDuty");
            ImGui.Text("");

            ImGui.Text("Requires ");
            StatusText.Draw(LifestreamIPC.IsEnabled, "Lifestream");
            ImGui.Text(" and ");
            StatusText.Draw(VNavMeshIPC.IsEnabled, "VNavMesh");
            ImGui.Text(" to be enabled");
        });

        Disabled.Draw(!onlyStartAutoDutyFromSafezone, () =>
        {
            ImGui.Spacing();
            ImGui.Text("Safezone priority");
            InformationTooltip.Draw("The runner will try these safezones from top to bottom until one can be entered");

            List<Safezone> safezones = AWC.Config.GetSortedSafezones();
            for (var index = 0; index < safezones.Count; index++) {
                Safezone safezone = safezones[index];

                ImGui.PushID($"safezone-priority-{safezone}");

                Disabled.Draw(index == 0, () =>
                {
                    if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowUp)) {
                        AWC.Config.MoveSafezone(safezone, -1);
                    }
                });

                ImGui.SameLine(0f, 4f);
                Disabled.Draw(index == safezones.Count - 1, () =>
                {
                    if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowDown)) {
                        AWC.Config.MoveSafezone(safezone, 1);
                    }
                });

                ImGui.SameLine();
                ImGui.Text(safezone.GetName());

                ImGui.PopID();
            }
        });
    }
}
