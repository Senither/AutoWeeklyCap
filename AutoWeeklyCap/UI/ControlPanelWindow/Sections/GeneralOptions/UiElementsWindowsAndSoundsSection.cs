using AutoWeeklyCap.UI.Helpers;

namespace AutoWeeklyCap.UI.ControlPanelWindow.Sections.GeneralOptions;

internal static class UiElementsWindowsAndSoundsSection
{
    internal static void Draw()
    {
        bool openWindow = AWC.Config.OpenWindowOnStartup;
        if (ImGui.Checkbox("Open Character UI window on startup", ref openWindow)) {
            AWC.Config.OpenWindowOnStartup = openWindow;
        }

        bool useSliders = AWC.Config.UseSliders;
        if (ImGui.Checkbox("Slider inputs", ref useSliders)) {
            AWC.Config.UseSliders = useSliders;
        }

        InformationTooltip.Draw(
            "When enabled, ranged inputs will be shown as sliders\n" +
            "When disabled, ranged inputs will be shown as text inputs with increment and decrement step buttons"
        );

        bool autoResizeWindow = AWC.Config.AutoResizeCharacterWindow;
        if (ImGui.Checkbox("Auto-resize characters window", ref autoResizeWindow)) {
            AWC.Config.AutoResizeCharacterWindow = autoResizeWindow;
        }

        InformationTooltip.Draw(
            "When enabled, the character window will automatically adjust\n" +
            "its height to fit the visible characters and action buttons"
        );

        bool dtrBar = AWC.Config.ShowStatusInStatusBar;
        if (ImGui.Checkbox("Show status server Info bar", ref dtrBar)) {
            AWC.Config.ShowStatusInStatusBar = dtrBar;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("Adds a status indicator to the server status bar, allowing for quickly seeing");
            ImGui.Text("the runner status, and toggling the windows and runner statues");
        });

        Disabled.Draw(!dtrBar, () =>
        {
            ImGui.SameLine(0f, 20f);
            var iconsDtr = AWC.Config.ShowStatusAsIcons;
            if (ImGui.Checkbox("Show status as icons instead of text", ref iconsDtr)) {
                AWC.Config.ShowStatusAsIcons = iconsDtr;
            }
        });

        Card.Separator();

        ImGui.Text("UI Theme");

        ColorTheme selectedTheme = AWC.Config.SelectedColorTheme;
        if (ImGui.BeginCombo("###theme-selector", selectedTheme.GetName())) {
            foreach (var theme in Enum.GetValues(typeof(ColorTheme)).Cast<ColorTheme>()) {
                if (ImGui.Selectable(theme.GetName())) {
                    AWC.Config.SetColorTheme(theme);
                }
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();
        if (ImGui.ArrowButton("###previous-ui-theme", ImGuiDir.Left)) {
            AWC.Config.SetColorTheme(selectedTheme.GetPreviousTheme());
        }

        ImGuiEx.Tooltip("Previous theme");

        ImGui.SameLine(0f, 2f);
        if (ImGui.ArrowButton("###next-ui-theme", ImGuiDir.Right)) {
            AWC.Config.SetColorTheme(selectedTheme.GetNextTheme());
        }

        ImGuiEx.Tooltip("Next theme");

        Card.Separator();

        bool muteGameSoundsWhenRunning = AWC.Config.MuteGameSoundsWhenRunning;
        if (ImGui.Checkbox("Mute game audio when running", ref muteGameSoundsWhenRunning)) {
            AWC.Config.MuteGameSoundsWhenRunning = muteGameSoundsWhenRunning;

            if (AWC.Runner.State.IsRunning()) {
                AudioHelper.MuteMasterGameAudio(muteGameSoundsWhenRunning);
            }
        }

        InformationTooltip.Draw(
            "When enabled, the game \"Master Volume\" will be muted while\n" +
            "the runner is going, effectively muting all game audio"
        );
    }
}
