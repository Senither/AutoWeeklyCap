using Dalamud.Interface.Windowing;

namespace AutoWeeklyCap.Contracts.UI;

public abstract class ThemeWindow(string windowName) : Window(windowName)
{
    private IDisposable? _windowStyle;

    public override void PreDraw()
    {
        _windowStyle = Theme.Push();

        base.PreDraw();
    }

    public override void PostDraw()
    {
        _windowStyle?.Dispose();
        _windowStyle = null;

        base.PostDraw();
    }
}
