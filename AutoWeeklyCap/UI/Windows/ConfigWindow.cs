using AutoWeeklyCap.UI.ConfigWindow;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface.Windowing;

namespace AutoWeeklyCap.UI.Windows;

public class ConfigWindow : Window
{
    private string _view = "general_options";

    public ConfigWindow() : base("Auto Weekly Tomestone Settings")
    {
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(525, 350), MaximumSize = new Vector2(9999, 9999) };
    }

    public override void Draw()
    {
        using (Theme.Push()) {
            SidebarLayout.DrawSidebar(() =>
            {
                if (ImGui.Button("General Options")) {
                    _view = "general_options";
                }

                if (ImGui.Button("Duty Options")) {
                    _view = "duty_options";
                }

                if (ImGui.Button("Characters")) {
                    _view = "characters";
                }

                if (ImGui.Button("Runner Options")) {
                    _view = "runner_options";
                }

                if (ImGui.Button("Stop Actions")) {
                    _view = "stop_actions";
                }

                if (ImGui.Button("Manually reset Tomestones")) {
                    _view = "reset_tomestones";
                }
            });

            SidebarLayout.DrawContent(() =>
            {
                switch (_view) {
                    case "general_options":
                        GeneralOptionsUi.Draw();
                        break;

                    case "duty_options":
                        DutyOptionsUi.Draw();
                        break;

                    case "characters":
                        CharactersOptionUi.Draw();
                        break;

                    case "runner_options":
                        RunnerPrerequisitesUi.Draw();
                        break;

                    case "stop_actions":
                        StopActionsUi.Draw();
                        break;

                    case "reset_tomestones":
                        ResetWeeklyTomestonesUi.Draw();
                        break;
                }
            });
        }
    }

    public override void OnClose()
    {
        AWC.Config.Save();
    }
}
