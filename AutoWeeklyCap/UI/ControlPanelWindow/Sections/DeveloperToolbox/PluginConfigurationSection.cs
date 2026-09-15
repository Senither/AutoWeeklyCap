using AutoWeeklyCap.Config;

using ECommons.Configuration;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.DeveloperToolbox;

internal static class PluginConfigurationSection
{
    internal static void Draw()
    {
        ImGui.TextWrapped("This section can be used to export partial, or the full plugin config, or alternatively completely reset the config back to the default values.");
        ImGui.Spacing();

        if (ImGui.Button("Export Full Config")) {
            ImGui.SetClipboardText(EzConfig.DefaultSerializationFactory.Serialize(AWC.Config.JSONClone(), false));
            Notify.Info("Config copied to clipboard");
        }

        ImGui.SameLine();

        if (ImGui.Button("Export Partal Config")) {
            Configuration? config = AWC.Config.JSONClone();

            config.Characters = null!;
            config.CollectedTomes = null!;
            config.CharacterForSwap = null!;

            ImGui.SetClipboardText(EzConfig.DefaultSerializationFactory.Serialize(config, false));
            Notify.Info("Config copied to clipboard");
        }

        ImGui.SameLine();

        if (ImGui.Button("Reset Plugin Config") && ImGuiEx.Ctrl) {
            AWC.Instance.Configuration = new Configuration();
            Notify.Info("Config reset to default");
        }

        ImGuiEx.Tooltip("Hold down CTRL + Click to reset the plugin configuration to all the default values");

        if (ImGui.Button("Print Overridable Config Pairs")) {
            foreach (var (key, value) in ConfigOverrides.GetKeyValuePairs()) {
                AWC.Log.Debug($"OverridablePair: Key: {key}, Value: {value}");
            }
        }

        ImGuiEx.Tooltip("Prints the overridable pairs of keys and their current value of to the console");

        ImGui.SameLine();

        if (ImGui.Button("Remove Plugin Overrides") && ImGuiEx.Ctrl) {
            ConfigOverrides.Clear();
            Notify.Info("Config overrides have been cleared");
        }

        ImGuiEx.Tooltip("Hold down CTRL + Click to remove all the plugin overrides");
    }
}
