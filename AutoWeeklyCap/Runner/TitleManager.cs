namespace AutoWeeklyCap.Runner;

public static class TitleManager
{
    public class TemporaryTitle : IDisposable
    {
        public TemporaryTitle(BitmapFontIcon icon, string status, string statusShort)
        {
            AWC.TaskManager.Enqueue(() =>
            {
                StatusIcon = icon;
                Status = status;
                StatusShort = statusShort;
            }, "set temporary title");
        }

        public void Dispose()
        {
            AWC.TaskManager.Enqueue(Reset, "reset title manager");

            GC.SuppressFinalize(this);
        }
    }

    private static string? Status = null;
    private static string? StatusShort = null;
    private static BitmapFontIcon? StatusIcon = null;

    public static string? GetStatus()
        => Status ?? AWC.Runner.GetState().GetStatus(AWC.Runner.IsStopping(), AWC.Runner.GetCurrentCharacter());

    public static string? GetStatusShort()
        => StatusShort ?? AWC.Runner.GetState().GetStatusShort(AWC.Runner.IsStopping(), AWC.Runner.GetCurrentCharacter());

    public static BitmapFontIcon GetStatusIcon()
        => StatusIcon ?? AWC.Runner.GetState().GetStatusIcon(AWC.Runner.IsStopping());

    public static TemporaryTitle RegisterTitle(BitmapFontIcon icon, string status) => RegisterTitle(icon, status, status);

    public static TemporaryTitle RegisterTitle(BitmapFontIcon icon, string status, string statusShort)
    {
        return new TemporaryTitle(icon, status, statusShort);
    }

    public static bool Reset()
    {
        Status = null;
        StatusShort = null;
        StatusIcon = null;

        return true;
    }
}
