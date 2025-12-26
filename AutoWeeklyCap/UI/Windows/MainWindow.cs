using System;
using System.Numerics;
using AutoWeeklyCap.IPC;
using AutoWeeklyCap.UI.MainWindow;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ECommons.Configuration;
using ECommons.ImGuiMethods;

namespace AutoWeeklyCap.UI.Windows;

public class MainWindow : Window, IDisposable
{
    private TitleBarButton LockButton;

    public MainWindow(Plugin plugin) : base("Auto Weekly Tomestone Capper##main-window")
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
                if (m == ImGuiMouseButton.Left) plugin.ToggleConfigUi();
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
                    Plugin.Config.Window.Pin = !Plugin.Config.Window.Pin;
                    LockButton?.Icon = Plugin.Config.Window.Pin ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen;
                }
            },
            Icon = Plugin.Config.Window.Pin ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen,
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
        var name = $"{Plugin.Name} {Plugin.Version}";
        if (Plugin.Runner.IsRunning())
        {
            name += $" | {Plugin.Runner.GetStatus()}";
        }

        WindowName = $"{name}###AutoWeeklyCap";

        if (Plugin.Config.Window.Pin)
        {
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Plugin.Config.Window.Position);
            ImGui.SetNextWindowSize(Plugin.Config.Window.Size);
        }

        Flags = Plugin.Config.Window.Pin ? ImGuiWindowFlags.NoResize : ImGuiWindowFlags.None;
    }

    public override void Draw()
    {
        DrawPluginStatus();
        DrawHeaderActionButtons();

        ImGuiEx.EzTabBar("tabbar", "Test",
                         ("Characters", CharactersUI.Draw, null, true),
                         ("About", AboutTabUi.Draw, null, true)
        );

        if (!Plugin.Config.Window.Pin)
        {
            Plugin.Config.Window.Position = ImGui.GetWindowPos();
            Plugin.Config.Window.Size = ImGui.GetWindowSize();
        }
    }

    protected void DrawPluginStatus()
    {
        ImGui.TextUnformatted("AWC is");
        ImGui.SameLine(0f, 6f);

        if (Utils.IsRequiredPluginsEnabled())
            ImGui.TextColored(ImGuiColors.HealerGreen, "✓ Ready");
        else
            ImGui.TextColored(ImGuiColors.DalamudOrange, "X Unavailable");

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();

            ImGui.TextUnformatted("Plugins required for AWC to work:");

            DrawPluginStatusTooltipWithContent(AutoDutyIPC.IsEnabled, "AutoDuty");
            DrawPluginStatusTooltipWithContent(LifestreamIPC.IsEnabled, "Lifestream");

            ImGui.TextUnformatted("Optional plugins to enhance AWC:");

            DrawPluginStatusTooltipWithContent(Utils.IsPluginEnabled("BossModReborn"), "BossMod Reborn");

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
        var isEnabled = Utils.IsRequiredPluginsEnabled() && !Plugin.Runner.IsStopping();

        if (!isEnabled)
            ImGui.BeginDisabled();

        var buttonOffset = Plugin.Runner.IsRunning() ? 86 : 70;
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - buttonOffset + ImGui.GetStyle().ItemSpacing.X);

        if (Plugin.Runner.IsRunning())
        {
            if (ImGui.Button(" Stop Runner "))
            {
                Plugin.Runner.Stop();
            }
        }
        else
        {
            if (ImGui.Button(" Start Run "))
            {
                if (Utils.IsRequiredPluginsEnabled())
                {
                    Plugin.Runner.Start();
                }
            }
        }

        if (!isEnabled)
            ImGui.EndDisabled();
    }
}
