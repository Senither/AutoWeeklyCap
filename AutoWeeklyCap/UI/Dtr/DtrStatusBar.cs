using System.Threading;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace AutoWeeklyCap.UI.Dtr;

public class DtrStatusBar : IDisposable
{
    private const string DtrBarTitle = "Auto Weekly Capper";
    private const string DtrBarTooltip = "Click => toggle the character window\nCTRL + Click => toggle runner status";

    private Thread? dtrEntryLoadThread;
    private IDtrBarEntry? dtrEntry;

    public void Start()
    {
        dtrEntryLoadThread = new Thread(() =>
        {
            if (dtrEntry != null)
                return;

            try
            {
                dtrEntry = AWC.DtrBar.Get(DtrBarTitle);
                dtrEntry.Text = "...";
                dtrEntry.Shown = false;
                dtrEntry.OnClick = _ => OnClick();
                dtrEntry.Tooltip = DtrBarTooltip;
            }
            catch (Exception e)
            {
                AWC.Log.Error(e, $"Failed to acquire DtrBarEntry {DtrBarTitle}");
                Thread.Sleep(100);
            }
        });

        dtrEntryLoadThread.Start();
    }

    public void Draw()
    {
        if (dtrEntry == null)
            return;

        if (!AWC.Config.ShowStatusInStatusBar)
        {
            if (dtrEntry.Shown)
                dtrEntry.Shown = false;

            return;
        }

        if (!EzThrottler.Throttle(nameof(DtrStatusBar), 250))
            return;

        dtrEntry.Tooltip = AWC.Config.ShowStatusAsIcons
                               ? $"Status: {TitleManager.GetStatusShort()}\n\n{DtrBarTooltip}"
                               : DtrBarTooltip;

        dtrEntry?.Shown = true;
        dtrEntry?.Text = new SeString(
            new TextPayload($"AWC: "),
            AWC.Config.ShowStatusAsIcons
                ? new IconPayload(TitleManager.GetStatusIcon())
                : new TextPayload(TitleManager.GetStatusShort())
        );
    }

    public void OnClick()
    {
        if (!ImGui.GetIO().KeyCtrl)
        {
            AWC.Instance.ToggleMainUi();
            return;
        }

        if (!AWC.Runner.IsRunning())
        {
            AWC.Runner.Start();
            return;
        }

        if (AWC.Runner.IsStopping())
            AWC.Runner.Resume();
        else
            AWC.Runner.Stop();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        dtrEntryLoadThread?.Join();
        dtrEntry?.Remove();
    }
}
