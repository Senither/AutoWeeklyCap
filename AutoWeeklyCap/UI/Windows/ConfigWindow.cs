using AutoWeeklyCap.UI.ConfigWindow;
using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface.Windowing;

namespace AutoWeeklyCap.UI.Windows;

public class ConfigWindow : Window
{
    private SettingsWindowOption _option = SettingsWindowOption.GeneralOptions;

    public ConfigWindow() : base("Auto Weekly Tomestone Settings")
    {
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(525, 350), MaximumSize = new Vector2(9999, 9999) };
    }

    public override void Draw()
    {
        using (Theme.Push()) {
            SidebarLayout.DrawSidebar(() =>
            {
                foreach (var option in Enum.GetValues(typeof(SettingsWindowOption)).Cast<SettingsWindowOption>()) {
                    if (MenuButton.Draw(option.GetIcon(), option.GetName(), _option == option)) {
                        _option = option;
                    }
                }
            });

            SidebarLayout.DrawContent(() => _option.Draw());
        }
    }

    public override void OnClose()
    {
        AWC.Config.Save();
    }
}
