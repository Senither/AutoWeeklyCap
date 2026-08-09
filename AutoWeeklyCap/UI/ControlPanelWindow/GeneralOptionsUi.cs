using AutoWeeklyCap.UI.Helpers;
using AutoWeeklyCap.UI.Windows;

using Range = AutoWeeklyCap.UI.Helpers.Range;

namespace AutoWeeklyCap.UI.ControlPanelWindow;

public static class GeneralOptionsUi
{
    public static void Draw()
    {
        Card.Draw("General Options", GeneralOptions, defaultOpen: true);
        Card.Draw("UI Elements, Windows & Sounds", UiElementsWindowsAndSounds, defaultOpen: true);
        Card.Draw("Status Icon", StatusIcon, defaultOpen: true);
        Card.Draw("Network Options", NetworkOptions, defaultOpen: true);
        Card.DrawWarning("Reset Weekly Tomestones", ResetWeeklyTomestones);
    }

    private static void GeneralOptions()
    {
        var startOnBoot = AWC.Config.StartRunnerOnBoot;
        if (ImGui.Checkbox("Start runner automatically on startup", ref startOnBoot)) {
            AWC.Config.StartRunnerOnBoot = startOnBoot;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When the option is enabled and at least one enabled character is still not");
            ImGui.Text("tome capped, AWC will automatically start the runner during game boot.");
            ImGui.Text("");
            ImGui.Text("Note: If all enabled characters are tome caped and unlimited mode is enabled");
            ImGui.Text("the runner will still not be automatically started during game boot.");
        });

        var trackDisabled = AWC.Config.TrackDisabledCharacters;
        if (ImGui.Checkbox("Track tomestones for disabled characters", ref trackDisabled)) {
            AWC.Config.TrackDisabledCharacters = trackDisabled;
        }
    }

    private static void UiElementsWindowsAndSounds()
    {
        var openWindow = AWC.Config.OpenWindowOnStartup;
        if (ImGui.Checkbox("Open Character UI window on startup", ref openWindow)) {
            AWC.Config.OpenWindowOnStartup = openWindow;
        }

        var useSliders = AWC.Config.UseSliders;
        if (ImGui.Checkbox("Slider inputs", ref useSliders)) {
            AWC.Config.UseSliders = useSliders;
        }

        InformationTooltip.Draw(
            "When enabled, ranged inputs will be shown as sliders\n" +
            "When disabled, ranged inputs will be shown as text inputs with increment and decrement step buttons"
        );

        var autoResizeWindow = AWC.Config.AutoResizeCharacterWindow;
        if (ImGui.Checkbox("Auto-resize characters window", ref autoResizeWindow)) {
            AWC.Config.AutoResizeCharacterWindow = autoResizeWindow;
        }

        InformationTooltip.Draw(
            "When enabled, the character window will automatically adjust\n" +
            "its height to fit the visible characters and action buttons"
        );

        var dtrBar = AWC.Config.ShowStatusInStatusBar;
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

        var selectedTheme = AWC.Config.SelectedColorTheme;
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

        var muteGameSoundsWhenRunning = AWC.Config.MuteGameSoundsWhenRunning;
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

    private static void StatusIcon()
    {
        if (ImGui.Checkbox("Display status icon", ref AWC.Config.StatusOverlayEnabled)) {
            if (AWC.Config.StatusOverlayEnabled) {
                StatusOverlayWindow.DrawOverlayPreview();
            } else {
                StatusOverlayWindow.CancelDrawingOverlayPreview();
            }
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled, a status overlay icon will be displayed when AWC is running, or preforming");
            ImGui.Text("any actions (repairing gear, extracting materia, spending tomestones, etc) that can");
            ImGui.Text("be used to quickly toggle windows on and off, or outright stopping the runner");
        });

        ImGui.Text("Icon size");
        var statusOverlayImageSize = AWC.Config.StatusOverlayImageSize;
        if (Range.Draw("###icon-size", ref statusOverlayImageSize, 50, 250, "%upx")) {
            AWC.Config.StatusOverlayImageSize = statusOverlayImageSize;
            StatusOverlayWindow.DrawOverlayPreview();
        }

        ImGui.Text("Icon position");
        foreach (var position in Enum.GetValues(typeof(StatusOverlayPosition)).Cast<StatusOverlayPosition>()) {
            using (AWC.Config.StatusOverlayPosition == position ? Theme.PushSuccessButton() : null) {
                if (ImGuiEx.IconButton(position.GetIcon(), $"###icon-position-{position.GetName()}")) {
                    AWC.Config.StatusOverlayPosition = position;
                    StatusOverlayWindow.DrawOverlayPreview();
                }

                ImGuiEx.Tooltip($"Set position to {position.GetName()}");

                if (!position.IsRightMostPosition()) {
                    ImGui.SameLine();
                }
            }
        }
    }

    private static void NetworkOptions()
    {
        var recovery = AWC.Config.AttemptRecoveryFromDisconnects;
        if (ImGui.Checkbox("Recovery from disconnects", ref recovery)) {
            AWC.Config.AttemptRecoveryFromDisconnects = recovery;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled and a disconnect is detected while the runner is active, AWC will");
            ImGui.Text("attempt to log back into your character and restart the runner.");
            ImGui.Text("");
            ImGui.Text("Note: It's recommended that ");
            StatusText.Draw(NoKillPluginIPC.IsEnabled, "No Kill Plugin");
            ImGui.Text(" is enabled when using the feature");
            ImGui.Text("to allow recovering from prolonged internet loss without the game closing");
        });

        var titleMovie = AWC.Config.DisableTitleScreenMovie;
        if (ImGui.Checkbox("Disable title screen movie", ref titleMovie)) {
            AWC.Config.DisableTitleScreenMovie = titleMovie;
        }

        InformationTooltip.Draw(() =>
        {
            ImGui.Text("When enabled the title screen movie will be disabled, regardless");
            ImGui.Text("of if the runner is actually running or not.");
        });
    }

    private static void ResetWeeklyTomestones()
    {
        ImGui.TextWrapped(
            "The tomestones will reset automatically during the weekly reset, however, " +
            "if you want to reset the tomes manually you can use the button below."
        );

        ImGui.Spacing();
        ImGui.Spacing();

        ActionButton.Draw(
            "Reset Weekly Tomestones",
            "Hold down CTRL to reset your weekly tomestones",
            () => AWC.Config.CollectedTomes.Clear()
        );
    }
}
