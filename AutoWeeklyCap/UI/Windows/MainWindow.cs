using AutoWeeklyCap.UI.Helpers;
using AutoWeeklyCap.UI.MainWindow;

using Dalamud.Interface;
using Dalamud.Interface.Windowing;

using ECommons.Configuration;

namespace AutoWeeklyCap.UI.Windows;

public class MainWindow : Window
{
    private readonly TitleBarButton _lockButton;
    private const float MinimumWindowHeight = 135f;

    public MainWindow(AWC autoWeeklyCap) : base("Auto Weekly Tomestone Capper##main-window")
    {
        TitleBarButtons.Add(new TitleBarButton
        {
            Click = (m) =>
            {
                if (m == ImGuiMouseButton.Left) {
                    autoWeeklyCap.ToggleConfigUi();
                }
            },
            Icon = FontAwesomeIcon.Cog,
            IconOffset = new Vector2(2, 2),
            ShowTooltip = () => ImGui.SetTooltip("Open settings window")
        });

        TitleBarButtons.Add(new TitleBarButton
        {
            Click = (m) =>
            {
                if (m == ImGuiMouseButton.Left) {
                    autoWeeklyCap.ToggleFeedbackUi();
                }
            },
            Icon = FontAwesomeIcon.Inbox,
            IconOffset = new Vector2(2, 2),
            ShowTooltip = () => ImGui.SetTooltip("Send plugin feedback")
        });

        _lockButton = new TitleBarButton
        {
            Click = (m) =>
            {
                if (m != ImGuiMouseButton.Left) {
                    return;
                }

                AWC.Config.Window.Pin = !AWC.Config.Window.Pin;
                _lockButton?.Icon = AWC.Config.Window.Pin
                    ? FontAwesomeIcon.Lock
                    : FontAwesomeIcon.LockOpen;
            },
            Icon = AWC.Config.Window.Pin ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen,
            IconOffset = new Vector2(3, 2),
            ShowTooltip = () => ImGui.SetTooltip("Lock window position and size")
        };

        TitleBarButtons.Add(_lockButton);
    }

    public override void OnClose()
    {
        EzConfig.Save();
    }

    public override void PreDraw()
    {
        var name = $"{Constants.Name} {AWC.Version}";

        var status = TitleManager.GetStatus();
        if (status != null) {
            name += $" | {status}";
        }

        WindowName = $"{name}###AWC";

        if (!AWC.Config.Window.Pin) {
            Flags = ImGuiWindowFlags.None;
            SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(425, MinimumWindowHeight), MaximumSize = new Vector2(9999, 9999) };
            return;
        }

        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(AWC.Config.Window.Position);

        if (AWC.Config.AutoResizeCharacterWindow) {
            Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize;
            SizeConstraints = new WindowSizeConstraints { MinimumSize = AWC.Config.Window.Size with { Y = MinimumWindowHeight }, MaximumSize = AWC.Config.Window.Size with { Y = 9999f } };
            return;
        }

        Flags = ImGuiWindowFlags.NoResize;
        ImGui.SetNextWindowSize(AWC.Config.Window.Size);
    }

    public override void Draw()
    {
        using (Theme.Push(withBackground: false)) {
            DrawPluginStatus();
            DrawHeaderActionButtons();

            ImGui.Spacing();
            CharactersTabUi.Draw();
            ImGui.Spacing();

            AWC.Config.Window.Size = AWC.Config.AutoResizeCharacterWindow && !AWC.Config.Window.Pin
                ? ApplyAutoResizeHeight()
                : ImGui.GetWindowSize();

            if (AWC.Config.Window.Pin) {
                return;
            }

            AWC.Config.Window.Position = ImGui.GetWindowPos();
        }
    }

    private Vector2 ApplyAutoResizeHeight()
    {
        var windowSize = ImGui.GetWindowSize();
        var desiredHeight = MathF.Max(
            SizeConstraints?.MinimumSize.Y ?? MinimumWindowHeight,
            ImGui.GetCursorPosY() + ImGui.GetStyle().WindowPadding.Y
        );

        if (Math.Abs(windowSize.Y - desiredHeight) <= 0.5f) {
            return windowSize;
        }

        var updatedSize = windowSize with { Y = desiredHeight };
        ImGui.SetWindowSize(updatedSize);

        return updatedSize;
    }

    private void DrawPluginStatus()
    {
        ImGui.TextUnformatted("AWC is");
        ImGui.SameLine(0f, 6f);

        if (AWC.IsRequiredPluginsEnabled()) {
            ImGui.TextColored(Theme.TextSuccess, "✓ Ready");
        } else {
            ImGui.TextColored(Theme.TextWarning, "X Unavailable");
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left)) {
            AWC.Instance.OpenConfigUi(SettingsWindowOption.PluginInformationAndDependencies);
        }

        if (!ImGui.IsItemHovered()) {
            return;
        }

        ImGui.BeginTooltip();

        ImGui.TextUnformatted("Plugins required for AWC to work:");

        DrawPluginStatusTooltipWithContent(AutoDutyIPC.IsEnabled, "AutoDuty");
        DrawPluginStatusTooltipWithContent(LifestreamIPC.IsEnabled, "Lifestream");
        DrawPluginStatusTooltipWithContent(VNavMeshIPC.IsEnabled, "VNavMesh");

        ImGui.EndTooltip();
    }

    private void DrawPluginStatusTooltipWithContent(bool status, string name)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, status ? Theme.TextSuccess : Theme.TextWarning);
        ImGui.TextUnformatted(status ? " ✓" : " X");
        ImGui.PopStyleColor();
        ImGui.SameLine(0.0f, 0.0f);
        ImGui.TextUnformatted(" " + name);
    }

    private void DrawHeaderActionButtons()
    {
        var isEnabled = AWC.IsRequiredPluginsEnabled() && AWC.Config.IsRequiredSettingsSetup();

        if (!isEnabled) {
            ImGui.BeginDisabled();
        }

        if (AWC.Runner.IsRunning()) {
            if (ImGuiEx.Ctrl) {
                if (RightAlignedButton.Draw(" Force Stop Runner ")) {
                    AWC.Runner.Abort();
                }
            } else if (AWC.Runner.IsStopping()) {
                if (RightAlignedButton.Draw(" Resume Runner ")) {
                    AWC.Runner.Resume();
                }
            } else {
                if (RightAlignedButton.Draw(" Stop Runner ")) {
                    AWC.Runner.Stop();
                }
            }
        } else {
            if (RightAlignedButton.Draw(" Start Run ")) {
                if (AWC.IsRequiredPluginsEnabled()) {
                    AWC.Runner.Start();
                }
            }
        }

        if (!isEnabled) {
            ImGui.EndDisabled();
        }
    }
}
