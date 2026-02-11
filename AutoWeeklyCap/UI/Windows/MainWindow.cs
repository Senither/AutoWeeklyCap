using AutoWeeklyCap.UI.Helpers;
using AutoWeeklyCap.UI.MainWindow;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ECommons.Configuration;

namespace AutoWeeklyCap.UI.Windows;

public class MainWindow : Window, IDisposable
{
    private TitleBarButton LockButton;

    public MainWindow(AWC autoWeeklyCap) : base("Auto Weekly Tomestone Capper##main-window")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 125),
            MaximumSize = new Vector2(9999, 9999)
        };

        TitleBarButtons.Add(new TitleBarButton
        {
            Click = (m) =>
            {
                if (m == ImGuiMouseButton.Left) autoWeeklyCap.ToggleConfigUi();
            },
            Icon = FontAwesomeIcon.Cog,
            IconOffset = new Vector2(2, 2),
            ShowTooltip = () => ImGui.SetTooltip("Open settings window"),
        });

        LockButton = new TitleBarButton()
        {
            Click = (m) =>
            {
                if (m == ImGuiMouseButton.Left)
                {
                    AWC.Config.Window.Pin = !AWC.Config.Window.Pin;
                    LockButton?.Icon = AWC.Config.Window.Pin
                                           ? FontAwesomeIcon.Lock
                                           : FontAwesomeIcon.LockOpen;
                }
            },
            Icon = AWC.Config.Window.Pin ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen,
            IconOffset = new Vector2(3, 2),
            ShowTooltip = () => ImGui.SetTooltip("Lock window position and size"),
        };

        TitleBarButtons.Add(LockButton);
    }

    public void Dispose() { }

    public override void OnClose()
    {
        EzConfig.Save();
    }

    public override void PreDraw()
    {
        var name = $"{AWC.Name} {AWC.Version}";
        if (AWC.Runner.IsRunning())
        {
            name += $" | {AWC.Runner.GetStatus()}";
        }

        WindowName = $"{name}###AWC";

        if (AWC.Config.Window.Pin)
        {
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(AWC.Config.Window.Position);
            ImGui.SetNextWindowSize(AWC.Config.Window.Size);
        }

        Flags = AWC.Config.Window.Pin ? ImGuiWindowFlags.NoResize : ImGuiWindowFlags.None;
    }

    public override void Draw()
    {
        DrawPluginStatus();
        DrawHeaderActionButtons();

        var tabs = new List<(string name, Action function, Vector4? color, bool child)>
            { ("Characters", CharactersUI.Draw, null, true) };

        if (!AWC.Config.HideUiElementDependencies)
            tabs.Add(("Dependencies", DependenciesUI.Draw, null, true));

        tabs.Add(("About", AboutTabUi.Draw, null, true));

        if (!AWC.Config.HideUiElementChangelog)
            tabs.Add(("Changelog", ChangelogUI.Draw, null, true));

        if (AWC.Config.DevMode && AWC.Config.ShowUiElementDebug)
            tabs.Add(("Debug", DebugUI.Draw, null, true));

        ImGuiEx.EzTabBar("main-awc-tabbar", "Test", tabs.ToArray());

        if (!AWC.Config.Window.Pin)
        {
            AWC.Config.Window.Position = ImGui.GetWindowPos();
            AWC.Config.Window.Size = ImGui.GetWindowSize();
        }
    }

    protected void DrawPluginStatus()
    {
        ImGui.TextUnformatted("AWC is");
        ImGui.SameLine(0f, 6f);

        if (AWC.IsRequiredPluginsEnabled())
            ImGui.TextColored(ImGuiColors.HealerGreen, "✓ Ready");
        else
            ImGui.TextColored(ImGuiColors.DalamudOrange, "X Unavailable");

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();

            ImGui.TextUnformatted("Plugins required for AWC to work:");

            DrawPluginStatusTooltipWithContent(AutoDutyIPC.IsEnabled, "AutoDuty");
            DrawPluginStatusTooltipWithContent(LifestreamIPC.IsEnabled, "Lifestream");

            ImGui.EndTooltip();
        }
    }

    protected void DrawPluginStatusTooltipWithContent(bool status, string name)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, status ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed);
        ImGui.TextUnformatted(status ? " ✓" : " X");
        ImGui.PopStyleColor();
        ImGui.SameLine(0.0f, 0.0f);
        ImGui.TextUnformatted(" " + name);
    }

    protected void DrawHeaderActionButtons()
    {
        var isEnabled = AWC.IsRequiredPluginsEnabled() && AWC.Config.IsRequiredSettingsSetup();

        if (!isEnabled)
            ImGui.BeginDisabled();

        if (AWC.Runner.IsRunning())
        {
            if (AWC.Runner.IsStopping())
            {
                if (RightAlignedButton.Draw(" Resume Runner "))
                {
                    AWC.Runner.Resume();
                }
            }
            else
            {
                if (RightAlignedButton.Draw(" Stop Runner "))
                {
                    AWC.Runner.Stop();
                }
            }
        }
        else
        {
            if (RightAlignedButton.Draw(" Start Run "))
            {
                if (AWC.IsRequiredPluginsEnabled())
                {
                    AWC.Runner.Start();
                }
            }
        }

        if (!isEnabled)
            ImGui.EndDisabled();
    }
}
