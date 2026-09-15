using Dalamud.Plugin;

namespace AutoWeeklyCap.UI.Helpers;

public static class PluginSettingsButton
{
    public static void Draw(PluginInstallerHelper.PluginContext context)
    {
        IExposedPlugin? plugin = context.GetExposedPlugin();
        if (plugin is { IsLoaded: true }) {
            if (RightAlignedButton.Draw($"Open Settings###ExposedPluginSettings:{context.PluginName}")) {
                plugin.OpenConfigUi();
            }

            return;
        }

        if (RightAlignedButton.Draw($"Install Plugin###ExposedPluginSettings:{context.PluginName}")) {
            context.InstallPlugin();
        }
    }
}
