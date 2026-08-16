using AutoWeeklyCap.Config;

namespace AutoWeeklyCap.UI.Helpers;

public static class ConfigOverridesStatus
{
    public static void Draw(Action content)
    {
        Disabled.Draw(ConfigOverrides.IsLocked, () =>
        {
            if (ConfigOverrides.IsLocked) {
                ImGuiEx.TextCentered("The config is being controlled by a third-party plugin and are therefore locked");
                ImGuiEx.TextCentered("until the third-party plugin unlocks the config, or the runner stops");
                ImGui.Spacing();
            }

            content();
        });
    }
}
