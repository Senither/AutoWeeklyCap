namespace AutoWeeklyCap.Runner;

public static class TitleManager
{
    private class TemporaryTitle : IDisposable
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
        return _status ?? AWC.Runner.State.CurrentStage.GetStatus(AWC.Runner.State.StoppingGracefully, AWC.Runner.State.CurrentCharacter);
    }

    public static string? GetStatusShort()
    {
        return _statusShort ?? AWC.Runner.State.CurrentStage.GetStatusShort(AWC.Runner.State.StoppingGracefully, AWC.Runner.State.CurrentCharacter);
    }

    public static BitmapFontIcon GetStatusIcon()
    {
        return _statusIcon ?? AWC.Runner.State.CurrentStage.GetStatusIcon(AWC.Runner.State.StoppingGracefully);
    }

    public static IDisposable RegisterTitle(BitmapFontIcon icon, string status)
    {
        return RegisterTitle(icon, status, status);
    }

    public static IDisposable RegisterTitle(BitmapFontIcon icon, string status, string statusShort)
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
