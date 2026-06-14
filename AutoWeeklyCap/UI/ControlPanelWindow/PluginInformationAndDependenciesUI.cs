using AutoWeeklyCap.IPC.Wotsit;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Utility;

// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class PluginInformationAndDependenciesUI
{
    private static readonly List<PluginInstallerHelper.PluginContext> RequiredPlugins =
    [
        AutoDutyIPC.Context,
        LifestreamIPC.Context,
        VNavMeshIPC.Context,
    ];

    private static readonly List<PluginInstallerHelper.PluginContext> RecommendedPlugins =
    [
        BossModRebornIPC.Context,
        RotationSolverRebornIPC.Context
    ];

    private static readonly List<PluginInstallerHelper.PluginContext> OptionalPlugins =
    [
        AutoRetainerIPC.Context,
        DeliverooIPC.Context,
        NotificationMasterIPC.Context,
        NoKillPluginIPC.Context,
        WotsitIPC.Context,
        StylistIPC.Context,
        SkipCutsceneIPC.Context,
    ];

    public static void Draw()
    {
        DrawPluginAboutInformation();

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Separator();

        ImGui.TextWrapped($"{Constants.Name} requires the following plugins to work:");
        DrawPluginList(RequiredPlugins);

        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped($"{Constants.Name} recommends the following plugins for an ideal experience:");
        DrawPluginList(RecommendedPlugins);

        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped($"The following plugins are recommended, but not required:");
        DrawPluginList(OptionalPlugins);
    }

    private static void DrawPluginAboutInformation()
    {
        ImGuiHelpers.ScaledDummy(5f);
        ImGuiEx.TextCentered($"{Constants.Name} v{AWC.Version}");
        ImGuiHelpers.ScaledDummy(1f);

        ImGuiEx.TextCentered("Developed and published by Senither");
        ImGuiEx.TextCentered("Original idea by Tuffic");
        ImGuiEx.TextCentered("Additional ideas by Naru, Myuri & Yoite");

        ImGuiHelpers.ScaledDummy(5f);

        ImGuiEx.LineCentered(() =>
        {
            ThemeButton.Draw("Plugin List", "https://dalamud-plugins.senither.com");
            ImGui.SameLine();
            ThemeButton.Draw("Plugin Repository", "https://dalamud-plugins.senither.com/plugin/AutoWeeklyCap.json");
            ImGui.SameLine();
            ThemeButton.Draw("Source Code", "https://github.com/Senither/AutoWeeklyCap");
        });
    }

    private static void DrawPluginList(List<PluginInstallerHelper.PluginContext> plugins)
    {
        ImGui.Spacing();

        foreach (var plugin in plugins) {
            DrawPlugin(plugin);
            ImGui.Spacing();
        }
    }

    private static void DrawPlugin(PluginInstallerHelper.PluginContext pluginInfo)
    {
        ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPos().X + 10, ImGui.GetCursorPos().Y));

        var plugin = FindInstalledPlugin(pluginInfo.PluginName);
        DrawPluginStatusIcon(plugin != null);

        var indent = ImGui.GetCursorPosX();
        DrawPluginName(plugin, pluginInfo.DisplayName);

        ImGui.SetCursorPosX(indent);
        ImGui.TextWrapped(pluginInfo.Description);

        if (plugin != null) {
            return;
        }

        ImGui.SetCursorPosX(indent);

        if (pluginInfo.InstallUrl != null) {
            DrawActionButton(FontAwesomeIcon.Download, $"Install###Install{pluginInfo.PluginName}", () => pluginInfo.InstallPlugin());
        }

        if (pluginInfo.WebsiteUrl != null) {
            DrawLinkButton(FontAwesomeIcon.Globe, $"Open Website###WebsiteFor{pluginInfo.PluginName}", pluginInfo.WebsiteUrl);
        }

        if (pluginInfo.RepositoryUrl != null) {
            DrawLinkButton(FontAwesomeIcon.Code, $"Open Repository###RepositoryFor{pluginInfo.PluginName}", pluginInfo.RepositoryUrl);
        }

        ImGui.NewLine();
    }

    private static void DrawPluginStatusIcon(bool status)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, status ? Theme.TextSuccess : Theme.TextDanger);
        ImGuiEx.Icon(status ? FontAwesomeIcon.Check : FontAwesomeIcon.PlugCircleXmark);
        ImGui.PopStyleColor();

        ImGui.SameLine();
    }

    private static void DrawPluginName(IExposedPlugin? plugin, string name)
    {
        ImGui.Text(plugin == null ? name : $"{plugin.Name} v{plugin.Version}");
    }

    private static void DrawLinkButton(FontAwesomeIcon icon, string text, string url)
    {
        if (ImGuiEx.IconButtonWithText(icon, text)) {
            Util.OpenLink(url);
        }

        ImGui.SameLine();
    }

    private static void DrawActionButton(FontAwesomeIcon icon, string text, Action action)
    {
        if (ImGuiEx.IconButtonWithText(icon, text)) {
            action();
        }

        ImGui.SameLine();
    }

    private static IExposedPlugin? FindInstalledPlugin(string internalName)
    {
        return AWC.PluginInterface.InstalledPlugins.FirstOrDefault(x => x.InternalName == internalName && x.IsLoaded);
    }
}
