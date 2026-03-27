using AutoWeeklyCap.UI.Helpers;
using AutoWeeklyCap.UI.MainWindow;

using Dalamud.Interface;
using Dalamud.Interface.Windowing;

namespace AutoWeeklyCap.UI.Windows;

public class MainWindow : Window
{
    private readonly TitleBarButton _lockButton;

    public MainWindow(AWC autoWeeklyCap) : base("Auto Weekly Tomestone Capper##main-window")
    {
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(425, 135), MaximumSize = new Vector2(9999, 9999) };

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
        AWC.Config.Save();
    }

    public override void PreDraw()
    {
        var name = $"{Constants.Name} {AWC.Version}";

        var status = TitleManager.GetStatus();
        if (status != null) {
            name += $" | {status}";
        }

        WindowName = $"{name}###AWC";

        if (AWC.Config.Window.Pin) {
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(AWC.Config.Window.Position);
            ImGui.SetNextWindowSize(AWC.Config.Window.Size);
        }

        if (AWC.Config.Window.Pin) {
            Flags = ImGuiWindowFlags.NoResize;
            return;
        }

        if (AWC.Config.AutoResizeCharacterWindow) {
            Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize;
            return;
        }

        Flags = ImGuiWindowFlags.None;
    }

    public override void Draw()
    {
        using (Theme.Push(withBackground: false)) {
            DrawPluginStatus();
            DrawHeaderActionButtons();

            ImGui.Spacing();
            CharactersTabUi.Draw();
            ImGui.Spacing();

            if (AWC.Config.Window.Pin) {
                return;
            }

            AWC.Config.Window.Position = ImGui.GetWindowPos();
            AWC.Config.Window.Size = ImGui.GetWindowSize();
        }
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
            if (AWC.Runner.IsStopping()) {
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
