namespace AutoWeeklyCap.Runner;

public static class TitleManager
{
    public class TemporaryTitle : IDisposable
    {
        public TemporaryTitle(BitmapFontIcon icon, string status, string statusShort)
        {
            AWC.TaskManager.Enqueue(() =>
            {
                _statusIcon = icon;
                _status = status;
                _statusShort = statusShort;
            }, "set temporary title");
        }

        public void Dispose()
        {
            AWC.TaskManager.Enqueue(Reset, "reset title manager");

            GC.SuppressFinalize(this);
        }
    }

    private static string? _status = null;
    private static string? _statusShort = null;
    private static BitmapFontIcon? _statusIcon = null;

    public static string? GetStatus()
    {
        return _status ?? AWC.Runner.GetState().GetStatus(AWC.Runner.IsStopping(), AWC.Runner.GetCurrentCharacter());
    }

    public static string? GetStatusShort()
    {
        return _statusShort ?? AWC.Runner.GetState().GetStatusShort(AWC.Runner.IsStopping(), AWC.Runner.GetCurrentCharacter());
    }

    public static BitmapFontIcon GetStatusIcon()
    {
        return _statusIcon ?? AWC.Runner.GetState().GetStatusIcon(AWC.Runner.IsStopping());
    }

    public static TemporaryTitle RegisterTitle(BitmapFontIcon icon, string status)
    {
        return RegisterTitle(icon, status, status);
    }

    public static TemporaryTitle RegisterTitle(BitmapFontIcon icon, string status, string statusShort)
    {
        return new TemporaryTitle(icon, status, statusShort);
    }

    public static bool Reset()
    {
        _status = null;
        _statusShort = null;
        _statusIcon = null;

        return true;
    }
}
