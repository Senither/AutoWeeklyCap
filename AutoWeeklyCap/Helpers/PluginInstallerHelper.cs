using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Plugin;
using ECommons.Reflection;

namespace AutoWeeklyCap.Helpers;

public static class PluginInstallerHelper
{
    private const string MasterRepositoryUrl = "https://dalamud-plugins.senither.com/";
    private const int MaxInstallRetries = 5;
    private const int InstallThrottleMs = 250;

    public static bool InstallPlugin(string repositoryUrl, string internalName)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
        {
            AWC.Log.Debug("PluginInstaller: Plugin installation was attempted for plugin with empty repositoryUrl");
            return false;
        }

        if (string.IsNullOrWhiteSpace(internalName))
        {
            AWC.Log.Debug("PluginInstaller: Plugin installation was attempted for plugin with empty internalName");
            return false;
        }

        AWC.Log.Debug($"PluginInstaller: Starting plugin installation for {internalName} ({repositoryUrl})");

        var tries = 0;
        Task<bool>? installTask = null;
        IExposedPlugin? exposedPlugin = null;

        AWC.TaskManager.Insert(
            () =>
            {
                if (!EzThrottler.Throttle($"InstallPlugin:InProgress:{internalName}", InstallThrottleMs))
                    return false;

                if (tries > MaxInstallRetries)
                {
                    AWC.Log.Debug($"PluginInstaller: reached matched attempts for {internalName}, stopping installation");
                    Notify.Error($"Failed to install {internalName}");

                    return true;
                }

                exposedPlugin ??= AWC.PluginInterface.InstalledPlugins.FirstOrDefault(plugin => plugin.InternalName == internalName);
                if (exposedPlugin != null)
                {
                    if (installTask is { Result: true })
                    {
                        AWC.Log.Debug($"PluginInstaller: Successfully installed plugin {internalName}");
                        Notify.Success($"{internalName} has been installed");
                    }
                    else if (IPCSubscriber.IsReady(internalName))
                    {
                        AWC.Log.Debug($"PluginInstaller: {internalName} is ready");
                        Notify.Success($"{internalName} is already installed");
                    }
                    else
                    {
                        AWC.Log.Debug($"PluginInstaller: {internalName} already installed but not ready");
                        Notify.Warning($"{internalName} is already installed, please enable the plugin in /xlplugins");
                        AWC.PluginInterface.OpenPluginInstallerTo(PluginInstallerOpenKind.AllPlugins, internalName);
                    }

                    return true;
                }

                if (installTask == null)
                {
                    var installUrl = DalamudReflector.HasRepo(MasterRepositoryUrl) ? MasterRepositoryUrl : repositoryUrl;

                    AWC.Log.Debug($"PluginInstaller: Installing plugin {internalName} from {installUrl}");
                    installTask = DalamudReflector.AddPlugin(installUrl, internalName);

                    return false;
                }

                if (!installTask.IsCompleted)
                    return false;

                AWC.Log.Debug($"PluginInstaller: Failed to install plugin {internalName}");

                tries++;
                installTask = null;

                return false;
            }, $"install plugin: {internalName}"
        );

        return true;
    }
}
