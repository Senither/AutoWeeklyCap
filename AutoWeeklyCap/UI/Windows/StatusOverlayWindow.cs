using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Dalamud.Interface.Windowing;

using Microsoft.VisualBasic;

namespace AutoWeeklyCap.UI.Windows;

public class StatusOverlayWindow : Window
{
    private static bool DrawingPreview = false;
    private static CancellationTokenSource? DrawingCancellationTokenSource;

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

    public static void DrawOverlayPreview()
    {
        if (AWC.Runner.IsRunning()) {
            return;
        }

        CancelDrawingOverlayPreview();

        _ = HideOverlayPreviewAfterDelay(DrawingCancellationTokenSource!.Token);
    }

    public static void CancelDrawingOverlayPreview()
    {
        DrawingPreview = false;
        DrawingCancellationTokenSource?.Cancel();
        DrawingCancellationTokenSource?.Dispose();
        DrawingCancellationTokenSource = new CancellationTokenSource();
    }

    private static async Task HideOverlayPreviewAfterDelay(CancellationToken cancellationToken)
    {
        DrawingPreview = true;

        try {
            await Task.Delay(2500, cancellationToken);
        } catch (OperationCanceledException) {
            return;
        }

        DrawingPreview = false;
    }

    public override bool DrawConditions()
    {
        if (DrawingPreview) {
            return true;
        }

        return AWC.Config.StatusOverlayEnabled && (AWC.Runner.IsRunning() || AWC.TaskManager.IsBusy);
    }

    public override void Draw()
    {
        Position = AWC.Config.StatusOverlayPosition.GetVector2();

        CImGui.igBringWindowToDisplayBack(CImGui.igGetCurrentWindow());

        if (!ThreadLoadImageHandler.TryGetTextureWrap(ImageResourcePath, out var textureWrap)) {
            AWC.Log.Debug($"Failed to get texture wrap for image resources");
            return;
        }

        ImGui.Image(textureWrap.Handle, ImageSize);

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
            if (AWC.Runner.IsRunning() && AWC.Runner.IsStopping()) {
                AWC.Runner.Resume();
            } else {
                AWC.Runner.Stop();
            }
        }

        ImGui.SetTooltip(Strings.Join([
            $"Status: {TitleManager.GetStatusShort()}",
            "",
            "Left Click - Toggles main window",
            "CTRL + Left Click - Toggles settings window",
            "Right Click - Stops the runner and any action"
        ], "\n"));
    }

    private static Vector2 ImageSize => new(AWC.Config.StatusOverlayImageSize, AWC.Config.StatusOverlayImageSize);

    private static string ImageResourcePath => Path.Combine(
        Svc.PluginInterface.AssemblyLocation.DirectoryName!,
        "resources",
        AWC.Runner.IsStopping()
            ? "stopping.png"
            : "running.png"
    );
}
