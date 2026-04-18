using System.Threading.Tasks;

using Dalamud.Interface;
using Dalamud.Plugin;

using ECommons.Reflection;

namespace AutoWeeklyCap.Helpers;

public static class PluginInstallerHelper
{
    public class PluginContext
    {
        public readonly string PluginName;
        public readonly string Description;
        public readonly string DisplayName;
        public readonly string? WebsiteUrl;
        public readonly string? RepositoryUrl;
        public readonly string? InstallUrl;

        private readonly bool _nativeDalamudPlugin;

        public PluginContext(
            string pluginName,
            string description,
            string? displayName = null,
            string? websiteUrl = null,
            string? repositoryUrl = null,
            string? installUrl = null,
            bool nativeDalamudPlugin = false
        )
        {
            PluginName = pluginName;
            Description = description;
            DisplayName = displayName ?? pluginName;
            WebsiteUrl = websiteUrl;
            RepositoryUrl = repositoryUrl;

            _nativeDalamudPlugin = nativeDalamudPlugin;

            InstallUrl = installUrl ?? $"https://dalamud-plugins.senither.com/plugin/{PluginName}.json";
        }

        public bool InstallPlugin()
        {
            if (_nativeDalamudPlugin) {
                if (GetExposedPlugin() != null) {
                    Notify.Warning($"{PluginName} is already installed, please enabled it manually");
                } else {
                    Notify.Info($"{PluginName} is a native Dalamud plugin, please install it manually");
                }

                AWC.PluginInterface.OpenPluginInstallerTo(PluginInstallerOpenKind.AllPlugins, PluginName);
                return false;
            }

            if (InstallUrl == null) {
                return false;
            }

            if (!EzThrottler.Throttle($"InstallPlugin:Start:{PluginName}", 1500)) {
                return false;
            }

            return PluginInstallerHelper.InstallPlugin(InstallUrl, PluginName);
        }

        public IExposedPlugin? GetExposedPlugin()
        {
            return AWC.PluginInterface.InstalledPlugins.FirstOrDefault(plugin => plugin.InternalName == PluginName);
        }
    }

    private const string MasterRepositoryUrl = "https://dalamud-plugins.senither.com/";

    public static bool InstallPlugin(string repositoryUrl, string internalName)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl)) {
            AWC.Log.Debug("PluginInstaller: Plugin installation was attempted for plugin with empty repositoryUrl");
            return false;
        }

        if (string.IsNullOrWhiteSpace(internalName)) {
            AWC.Log.Debug("PluginInstaller: Plugin installation was attempted for plugin with empty internalName");
            return false;
        }

        var exposedPlugin = AWC.PluginInterface.InstalledPlugins.FirstOrDefault(plugin => plugin.InternalName == internalName);
        if (exposedPlugin != null) {
            if (IPCSubscriber.IsReady(internalName)) {
                AWC.Log.Debug($"PluginInstaller: {internalName} is ready");
                Notify.Success($"{internalName} is already installed");
            } else {
                AWC.Log.Debug($"PluginInstaller: {internalName} already installed but not ready");
                Notify.Warning($"{internalName} is already installed, please enable the plugin in /xlplugins");
                AWC.PluginInterface.OpenPluginInstallerTo(PluginInstallerOpenKind.AllPlugins, internalName);
            }

            return true;
        }

        var installUrl = DalamudReflector.HasRepo(MasterRepositoryUrl) ? MasterRepositoryUrl : repositoryUrl;

        if (!DalamudReflector.HasRepo(installUrl)) {
            AWC.Log.Debug($"PluginInstaller: Installing new repository ({installUrl}) for plugin {internalName}");
            DalamudReflector.AddRepo(installUrl, true);
        }

        AWC.PluginInterface.OpenPluginInstallerTo(PluginInstallerOpenKind.AllPlugins, internalName);
        Notify.Info($"Please install {internalName} to get started with using it");

        return true;
    }
}
