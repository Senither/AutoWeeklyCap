using System.Threading;

using Dalamud.Plugin.Ipc;

using ECommons.Events;
using ECommons.Schedulers;

namespace AutoWeeklyCap.IPC.Wotsit;

public class WotsitManager : IDisposable
{
    private readonly Dictionary<string, WotsitEntry> _registered = [];
    private HashSet<WotsitEntry> _lastEntries = [];
    private int _initializeWotsitRequestId;

    private readonly ICallGateSubscriber<bool>? _faAvailable;
    private readonly ICallGateSubscriber<string, bool>? _faInvoke;

    public WotsitManager()
    {
        _faAvailable = Svc.PluginInterface.GetIpcSubscriber<bool>("FA.Available");
        _faAvailable.Subscribe(OnAvailable);
        _faInvoke = Svc.PluginInterface.GetIpcSubscriber<string, bool>("FA.Invoke");
        _faInvoke.Subscribe(HandleInvoke);

        ProperOnLogin.RegisterAvailable(OnLogin, true);
        Svc.ClientState.TerritoryChanged += OnTerritoryChanged;
        Svc.ClientState.Logout += OnLogout;
    }

    public void Dispose()
    {
        ClearWotsit();

        _faAvailable?.Unsubscribe(OnAvailable);
        _faInvoke?.Unsubscribe(HandleInvoke);

        ProperOnLogin.Unregister(OnLogin);
        Svc.ClientState.TerritoryChanged -= OnTerritoryChanged;
        Svc.ClientState.Logout -= OnLogout;

        GC.SuppressFinalize(this);
    }

    private void OnAvailable()
    {
        InitializeWotsitWithDelay("FA.Available");
    }

    private void OnLogin()
    {
        InitializeWotsitWithDelay("ProperOnLogin.Available");
    }

    private void OnTerritoryChanged(uint territory)
    {
        InitializeWotsitWithDelay("OnTerritoryChanged");
    }

    private void OnLogout(int type, int code)
    {
        ClearWotsit();
    }

    private void InitializeWotsitWithDelay(string trigger)
    {
        var requestId = Interlocked.Increment(ref _initializeWotsitRequestId);
        var timeoutAt = DateTime.UtcNow.AddSeconds(25);

        TryInitialize();
        return;

        void TryInitialize()
        {
            if (requestId != _initializeWotsitRequestId) {
                return;
            }

            if (PlayerHelper.IsValid) {
                InitializeWotsit(trigger);
                return;
            }

            if (DateTime.UtcNow >= timeoutAt) {
                AWC.Log.Debug($"WotsitManager: Initialization timed out while waiting for a valid player state, trigger: {trigger}");
                return;
            }

            _ = new TickScheduler(TryInitialize, 250);
        }
    }

    public void InitializeWotsit(string trigger)
    {
        if (!WotsitIPC.IsEnabled) {
            ClearWotsit();
            return;
        }

        AWC.Log.Debug($"WotsitManager: Initializing triggered by: {trigger}, status: {WotsitIPC.IsEnabled}");

        if (!PlayerHelper.IsLoggedIn) {
            AWC.Log.Debug($"WotsitManager: Initializing stopped, player is not logged in");
            return;
        }

        var newEntries = WotsitEntryGenerator.Generate().ToHashSet();
        if (_lastEntries.Count != 0 && newEntries.SetEquals(_lastEntries)) {
            AWC.Log.Debug("WotsitManager: Entries have not changed, skipping re-registration");
            return;
        }

        ClearWotsit();

        var faRegisterWithSearch = Svc.PluginInterface.GetIpcSubscriber<string, string, string, uint, string>("FA.RegisterWithSearch");

        _lastEntries = newEntries;
        foreach (var entry in newEntries) {
            var id = faRegisterWithSearch.InvokeFunc(Constants.Name, entry.DisplayName, $"{Constants.Name} {entry.SearchString}", entry.IconId);
            _registered.Add(id, entry);

            AWC.Log.Debug($"WotsitManager: Invoked FA.RegisterWithSearch(\"{Constants.Name}\", \"{entry.DisplayName}\", \"{entry.SearchString}\", {entry.IconId}) => {id}");
        }
    }

    private void ClearWotsit()
    {
        Interlocked.Increment(ref _initializeWotsitRequestId);

        try {
            if (!WotsitIPC.IsEnabled) {
                return;
            }

            Svc.PluginInterface.GetIpcSubscriber<string, bool>("FA.UnregisterAll").InvokeFunc(Constants.Name);

            AWC.Log.Debug($"WotsitManager: Invoked FA.UnregisterAll(\"{Constants.Name}\")");
        } catch (Exception e) {
            AWC.Log.Warning($"WotsitManager: Failed to clear wotsit: {e}");
        } finally {
            _registered.Clear();
            _lastEntries.Clear();
        }
    }

    private void HandleInvoke(string id)
    {
        if (!_registered.TryGetValue(id, out var entry)) {
            return;
        }

        AWC.Log.Debug($"WotsitManager: Received FA.Invoke(\"{id}\") => {entry.DisplayName}");

        try {
            entry.Callback.DynamicInvoke();
        } catch (Exception e) {
            AWC.Log.Error($"WotsitManager: Could not handle FA.Invoke(\"{id}\") ({entry.DisplayName}): {e}");
        }
    }
}
