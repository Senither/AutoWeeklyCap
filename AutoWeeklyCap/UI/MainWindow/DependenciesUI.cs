using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Utility;

// ReSharper disable InconsistentNaming

namespace AutoWeeklyCap.UI.MainWindow;

internal static class DependenciesUI
{
    public class PluginInformation
    {
        public readonly string PluginName;
        public readonly string Description;
        public readonly string? WebsiteUrl;
        public readonly string? RepositoryUrl;
        public readonly string? InstallUrl;

        public PluginInformation(
            string pluginName,
            string description,
            string? websiteUrl = null,
            string? repositoryUrl = null,
            string? installUrl = null
        )
        {
            PluginName = pluginName;
            Description = description;
            WebsiteUrl = websiteUrl;
            RepositoryUrl = repositoryUrl;

            InstallUrl = installUrl ?? $"https://dalamud-plugins.senither.com/plugin/{PluginName}.json";
        }

        public bool InstallPlugin()
        {
            if (InstallUrl == null)
                return false;

            if (!EzThrottler.Throttle($"InstallPlugin:Start:{PluginName}", 1500))
                return false;

            return PluginInstallerHelper.InstallPlugin(InstallUrl, PluginName);
        }
    }

    private static readonly List<PluginInformation> RequiredPlugins =
    [
        new(
            pluginName: AutoDutyIPC.Name,
            description: "Used to run the duties when farming tomestones.",
            repositoryUrl: "https://github.com/erdelf/AutoDuty"
        ),
        new(
            pluginName: LifestreamIPC.Name,
            description: "Used to travel to aethernet shards in cities, and switch between characters.",
            repositoryUrl: "https://github.com/NightmareXIV/Lifestream"
        ),
    ];

    private static readonly List<PluginInformation> RecommendedPlugins =
    [
        new(
            pluginName: BossModRebornIPC.Name,
            description: "Better combat AI for dodging and avoiding attacks while in duties.",
            repositoryUrl: "https://github.com/FFXIV-CombatReborn/BossmodReborn"
        ),
        new(
            pluginName: RotationSolverRebornIPC.Name,
            description: "Better combat rotation solver, making duty runs quicker and more seamless.",
            repositoryUrl: "https://github.com/FFXIV-CombatReborn/RotationSolverReborn"
        ),
    ];

    private static readonly List<PluginInformation> OptionalPlugins =
    [
        new(
            pluginName: AutoRetainerIPC.Name,
            description: "Used to mange retainer ventures and deployables on all your characters.",
            websiteUrl: "https://puni.sh/",
            repositoryUrl: "https://github.com/PunishXIV/AutoRetainer"
        ),
        new(
            pluginName: DeliverooIPC.Name,
            description: "Used to automate your grand company deliveries to get GC seals, and spend them to buy your preferred items.",
            repositoryUrl: "https://github.com/VeraNala/Deliveroo"
        ),
        new(
            pluginName: NotificationMasterIPC.Name,
            description: "Used to send notifications outside the game to notify you when the runner is done, such as making the game icon in the taskbar flash, sending toast notifications, and playing audio.",
            repositoryUrl: "https://github.com/NightmareXIV/NotificationMaster"
        ),
        new(
            pluginName: NoKillPluginIPC.Name,
            description: "Prevents the game from closing when getting lobby errors (Prolonged network issues)",
            repositoryUrl: "https://github.com/Bluefissure/NoKillPlugin"
        ),
        new(
            pluginName: VNavMeshIPC.Name,
            description: "Handles navigating within a zone, moving your character to retainer bells and NPCs for repairs or buying materials.",
            repositoryUrl: "https://github.com/awgil/ffxiv_navmesh"
        ),
    ];

    public static void Draw()
    {
        ImGui.TextWrapped($"{AWC.Name} requires the following plugins to work:");
        DrawPluginList(RequiredPlugins);

        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped($"{AWC.Name} recommends the following plugins for an ideal experience:");
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

        if (pluginInfo.InstallUrl != null)
            DrawActionButton(FontAwesomeIcon.Download, $"Install###Install{pluginInfo.PluginName}", () => pluginInfo.InstallPlugin());

        if (pluginInfo.WebsiteUrl != null)
            DrawLinkButton(FontAwesomeIcon.Globe, $"Open Website###WebsiteFor{pluginInfo.PluginName}", pluginInfo.WebsiteUrl);

        if (pluginInfo.RepositoryUrl != null)
            DrawLinkButton(FontAwesomeIcon.Code, $"Open Repository###RepositoryFor{pluginInfo.PluginName}", pluginInfo.RepositoryUrl);

        ImGui.NewLine();
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
            Util.OpenLink(url);

        ImGui.SameLine();
    }

    private static void DrawActionButton(FontAwesomeIcon icon, string text, Action action)
    {
        if (ImGuiEx.IconButtonWithText(icon, text))
            action();

        ImGui.SameLine();
    }

    private static IExposedPlugin? FindInstalledPlugin(string internalName)
    {
        return AWC.PluginInterface.InstalledPlugins.FirstOrDefault(x => x.InternalName == internalName && x.IsLoaded);
    }
}
