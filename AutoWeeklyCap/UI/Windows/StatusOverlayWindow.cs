using System.IO;

using Dalamud.Interface.Windowing;

using Microsoft.VisualBasic;

namespace AutoWeeklyCap.UI.Windows;

public class StatusOverlayWindow : Window
{
    public StatusOverlayWindow() : base("awc##overlay-window")
    {
        Flags = ImGuiWindowFlags.NoFocusOnAppearing |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.AlwaysAutoResize;

        IsOpen = true;
        ForceMainWindow = true;
        ShowCloseButton = false;
        RespectCloseHotkey = false;
    }

    public override bool DrawConditions()
    {
        return AWC.Runner.IsRunning() || AWC.TaskManager.IsBusy;
    }

    public override void Draw()
    {
        CImGui.igBringWindowToDisplayBack(CImGui.igGetCurrentWindow());
        Position = StatusOverlayPosition.TopLeft.GetVector2();

        if (!ThreadLoadImageHandler.TryGetTextureWrap(GetImageResource(), out var textureWrap)) {
            AWC.Log.Debug($"Failed to get texture wrap for image resources");
            return;
        }

        // TODO: Add image size to the config so it's user controllable
        ImGui.Image(textureWrap.Handle, new Vector2(94f, 94f));

        if (!ImGui.IsItemHovered()) {
            return;
        }

        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left)) {
            if (ImGuiEx.Ctrl) {
                AWC.Instance.ToggleConfigUi();
            } else {
                AWC.Instance.ToggleMainUi();
            }
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            AWC.Runner.Abort();
        }

        ImGui.SetTooltip(Strings.Join([
            $"Status: {TitleManager.GetStatusShort()}",
            "",
            "Left Click - Toggles main window",
            "CTRL + Left Click - Toggles settings window",
            "Right Click - Stops the runner and any action"
        ], "\n"));
    }

    private static string GetImageResource()
    {
        return Path.Combine(
            Svc.PluginInterface.AssemblyLocation.DirectoryName!,
            "resources",
            AWC.Runner.IsStopping()
                ? "stopping.png"
                : "running.png"
        );
    }
}
