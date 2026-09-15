using AutoWeeklyCap.UI.Helpers;
using AutoWeeklyCap.UI.Windows;

using Range = AutoWeeklyCap.UI.Helpers.Range;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.GeneralOptions;

internal static class StatusIconSection
{
    internal static void Draw()
    {
        if (ImGui.Checkbox("Display status icon", ref AWC.Config.StatusOverlayEnabled)) {
            if (AWC.Config.StatusOverlayEnabled) {
                StatusOverlayWindow.DrawOverlayPreview();
            } else {
                StatusOverlayWindow.CancelDrawingOverlayPreview();
            }
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled, a status overlay icon will be displayed when AWC is running, or preforming");
            ImGui.Text("any actions (repairing gear, extracting materia, spending tomestones, etc) that can");
            ImGui.Text("be used to quickly toggle windows on and off, or outright stopping the runner");
        });

        ImGui.Text("Icon size");
        uint statusOverlayImageSize = AWC.Config.StatusOverlayImageSize;
        if (Range.Draw("###icon-size", ref statusOverlayImageSize, 50, 250, "%upx")) {
            AWC.Config.StatusOverlayImageSize = statusOverlayImageSize;
            StatusOverlayWindow.DrawOverlayPreview();
        }

        ImGui.Text("Icon position");
        foreach (var position in Enum.GetValues(typeof(StatusOverlayPosition)).Cast<StatusOverlayPosition>()) {
            using (AWC.Config.StatusOverlayPosition == position ? Theme.PushSuccessButton() : null) {
                if (ImGuiEx.IconButton(position.GetIcon(), $"###icon-position-{position.GetName()}")) {
                    AWC.Config.StatusOverlayPosition = position;
                    StatusOverlayWindow.DrawOverlayPreview();
                }

                ImGuiEx.Tooltip($"Set position to {position.GetName()}");

                if (!position.IsRightMostPosition()) {
                    ImGui.SameLine();
                }
            }
        }
    }
}
