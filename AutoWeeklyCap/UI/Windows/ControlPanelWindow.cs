using AutoWeeklyCap.UI.Helpers;

using Dalamud.Interface.Windowing;

namespace AutoWeeklyCap.UI.Windows;

public class ControlPanelWindow : Window
{
    private SettingsWindowOption _option = SettingsWindowOption.GeneralOptions;

    public ControlPanelWindow() : base("Auto Weekly Cap Control Panel")
    {
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(575, 400), MaximumSize = new Vector2(9999, 9999) };
    }

    public override void Draw()
    {
        using (Theme.Push()) {
            SidebarLayout.DrawSidebar(() =>
            {
                foreach (var option in Enum.GetValues(typeof(SettingsWindowOption)).Cast<SettingsWindowOption>()) {
                    if (!option.IsDrawable()) {
                        continue;
                    }

                    if (MenuButton.Draw(option.GetIcon(), option.GetName(), _option == option, widthBreakpoint: SidebarLayout.GetSidebarContentTextBreakpoint())) {
                        SetCurrentTab(option);
                    }
                }
            });

            SidebarLayout.DrawContent(() => _option.Draw());
        }
    }

    public void SetCurrentTab(SettingsWindowOption option)
    {
        _option = option;
    }

    public override void OnClose()
    {
        AWC.Config.Save();
    }
}
