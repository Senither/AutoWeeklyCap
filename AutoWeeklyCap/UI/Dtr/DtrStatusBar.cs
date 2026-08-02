using System.Threading;

using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace AutoWeeklyCap.UI.Dtr;

public class DtrStatusBar : IDisposable
{
    private const string DtrBarTitle = "Auto Weekly Capper";
    private const string DtrBarTooltip = "Click => toggle the character window\n";
    private const string DtrBarNormalAction = "CTRL + Click => toggle runner status";
    private const string DtrBarCancelAction = "CTRL + Click => cancel current action";

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

        var tooltip = AWC.Config.ShowStatusAsIcons
            ? $"Status: {TitleManager.GetStatusShort()}\n\n{DtrBarTooltip}"
            : DtrBarTooltip;

        tooltip += (!AWC.Runner.State.IsRunning() && AWC.TaskManager.IsBusy)
            ? DtrBarCancelAction
            : DtrBarNormalAction;

        _dtrEntry.Tooltip = tooltip;

        _dtrEntry?.Shown = true;
        _dtrEntry?.Text = new SeString(
            new TextPayload($"AWC: "),
            AWC.Config.ShowStatusAsIcons
                ? new IconPayload(TitleManager.GetStatusIcon())
                : new TextPayload(TitleManager.GetStatusShort())
        );
    }

    private static void OnClick()
    {
        if (!ImGui.GetIO().KeyCtrl) {
            AWC.Instance.ToggleMainUi();
            return;
        }

        if (!AWC.IsRequiredPluginsEnabled() || !AWC.Config.IsRequiredSettingsSetup()) {
            Notify.Warning("Failed to start AWC, some required plugins are missing");
            AWC.Instance.OpenMainUi();
            return;
        }

        if (!AWC.Runner.State.IsRunning()) {
            if (AWC.TaskManager.IsBusy) {
                AWC.Runner.Abort();
            } else {
                AWC.Runner.Start();
            }
        } else if (AWC.Runner.State.StoppingGracefully) {
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
