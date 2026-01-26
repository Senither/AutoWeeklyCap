using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AutoWeeklyCap.IPC;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin;
using Dalamud.Utility;
using ECommons.ImGuiMethods;

namespace AutoWeeklyCap.UI.MainWindow;

internal record PluginInformation(string PluginName, string Description, string? WebsiteUrl = null, string? RepositoryUrl = null)
{
    public readonly string PluginName = PluginName;
    public readonly string Description = Description;
    public readonly string? WebsiteUrl = WebsiteUrl;
    public readonly string? RepositoryUrl = RepositoryUrl;
}

internal static class DependenciesUI
{
    private static readonly List<PluginInformation> RequiredPlugins =
    [
        new(
            PluginName: AutoDutyIPC.Name,
            Description: "Used to run the duties when farming tomestones.",
            RepositoryUrl: "https://github.com/erdelf/AutoDuty"
        ),
        new(
            PluginName: LifestreamIPC.Name,
            Description: "Used to travel to aethernet shards in cities, and switch between characters.",
            RepositoryUrl: "https://github.com/NightmareXIV/Lifestream"
        ),
    ];

    private static readonly List<PluginInformation> RecommendedPlugins =
    [
        new(
            PluginName: BossModReborn.Name,
            Description: "Better combat AI for dodging and avoiding attacks while in duties.",
            RepositoryUrl: "https://github.com/FFXIV-CombatReborn/BossmodReborn"
        ),
        new(
            PluginName: RotationSolverReborn.Name,
            Description: "Better combat rotation solver, making duty runs quicker and more seamless.",
            RepositoryUrl: "https://github.com/FFXIV-CombatReborn/RotationSolverReborn"
        ),
    ];

    private static readonly List<PluginInformation> OptionalPlugins =
    [
        new(
            PluginName: AutoRetainerIPC.Name,
            Description: "Used to mange retainer ventures and deployables on all your characters.",
            WebsiteUrl: "https://puni.sh/",
            RepositoryUrl: "https://github.com/PunishXIV/AutoRetainer"
        ),
        new(
            PluginName: NoKillPlugin.Name,
            Description: "Prevents the game from closing when getting lobby errors (Prolonged network issues)",
            RepositoryUrl: "https://github.com/Bluefissure/NoKillPlugin"
        ),
        new(
            PluginName: VNavMeshIPC.Name,
            Description: "Handles navigating within a zone, moving your character to retainer bells and NPCs for repairs or buying materials.",
            RepositoryUrl: "https://github.com/awgil/ffxiv_navmesh"
        ),
    ];

    public static void Draw()
    {
        ImGui.TextWrapped($"{AutoWeeklyCap.Name} requires the following plugins to work:");
        DrawPluginList(RequiredPlugins);

        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped($"{AutoWeeklyCap.Name} recommends the following plugins for an ideal experience:");
        DrawPluginList(RecommendedPlugins);

        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped($"The following plugins are recommended, but not required:");
        DrawPluginList(OptionalPlugins);
    }

    private static void DrawPluginList(List<PluginInformation> plugins)
    {
        ImGui.Spacing();

        foreach (var plugin in plugins)
        {
            DrawPlugin(plugin);
            ImGui.Spacing();
        }
    }

    private static void DrawPlugin(PluginInformation pluginInfo)
    {
        ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPos().X + 10, ImGui.GetCursorPos().Y));

        var plugin = FindInstalledPlugin(pluginInfo.PluginName);
        DrawPluginStatusIcon(plugin != null);
        
        var indent = ImGui.GetCursorPosX();
        DrawPluginName(plugin, pluginInfo.PluginName);

        ImGui.SetCursorPosX(indent);
        ImGui.TextWrapped(pluginInfo.Description);

        if (plugin != null)
            return;

        ImGui.SetCursorPosX(indent);

        if (pluginInfo.WebsiteUrl != null)
            DrawLinkButton(FontAwesomeIcon.Globe, "Open Website", pluginInfo.WebsiteUrl);

        if (pluginInfo is { WebsiteUrl: not null, RepositoryUrl: not null })
            ImGui.SameLine();

        if (pluginInfo.RepositoryUrl != null)
            DrawLinkButton(FontAwesomeIcon.Code, "Open Repository", pluginInfo.RepositoryUrl);
    }

    private static void DrawPluginStatusIcon(bool status)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, status ? ImGuiColors.HealerGreen : ImGuiColors.DPSRed);
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
        if (ImGuiEx.IconButtonWithText(icon, text))
        {
            Util.OpenLink(url);
        }
    }

    private static IExposedPlugin? FindInstalledPlugin(string internalName)
    {
        return AutoWeeklyCap.PluginInterface.InstalledPlugins.FirstOrDefault(x => x.InternalName == internalName && x.IsLoaded);
    }
}
