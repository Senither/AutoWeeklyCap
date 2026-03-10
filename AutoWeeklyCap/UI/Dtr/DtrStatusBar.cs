using System.Threading;

using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace AutoWeeklyCap.UI.Dtr;

public class DtrStatusBar : IDisposable
{
    private const string DtrBarTitle = "Auto Weekly Capper";
    private const string DtrBarTooltip = "Click => toggle the character window\nCTRL + Click => toggle runner status";

    private Thread? _dtrEntryLoadThread;
    private IDtrBarEntry? _dtrEntry;

    public void Start()
    {
        _dtrEntryLoadThread = new Thread(() =>
        {
            if (_dtrEntry != null) {
                return;
            }

            try {
                _dtrEntry = AWC.DtrBar.Get(DtrBarTitle);
                _dtrEntry.Text = "...";
                _dtrEntry.Shown = false;
                _dtrEntry.OnClick = _ => OnClick();
                _dtrEntry.Tooltip = DtrBarTooltip;
            } catch (Exception e) {
                AWC.Log.Error(e, $"Failed to acquire DtrBarEntry {DtrBarTitle}");
            }
        });

        _dtrEntryLoadThread.Start();
    }

    public void Draw()
    {
        if (_dtrEntry == null) {
            return;
        }

        if (!AWC.Config.ShowStatusInStatusBar) {
            if (_dtrEntry.Shown) {
                _dtrEntry.Shown = false;
            }

            return;
        }

        if (!EzThrottler.Throttle(nameof(DtrStatusBar), 250)) {
            return;
        }

        _dtrEntry.Tooltip = AWC.Config.ShowStatusAsIcons
            ? $"Status: {TitleManager.GetStatusShort()}\n\n{DtrBarTooltip}"
            : DtrBarTooltip;

        _dtrEntry?.Shown = true;
        _dtrEntry?.Text = new SeString(
            new TextPayload($"AWC: "),
            AWC.Config.ShowStatusAsIcons
                ? new IconPayload(TitleManager.GetStatusIcon())
                : new TextPayload(TitleManager.GetStatusShort())
        );
    }

    public void OnClick()
    {
        if (!ImGui.GetIO().KeyCtrl) {
            AWC.Instance.ToggleMainUi();
            return;
        }

        if (!AWC.Runner.IsRunning()) {
            AWC.Runner.Start();
            return;
        }

        if (AWC.Runner.IsStopping()) {
            AWC.Runner.Resume();
        } else {
            AWC.Runner.Stop();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _dtrEntryLoadThread?.Join();
        _dtrEntry?.Remove();
    }
}
